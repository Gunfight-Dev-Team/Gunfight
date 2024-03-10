Shader "Hidden/Hologram" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _HologramStripesAmount ("Stripes Amount", Range(0, 1)) = 0.1 //条纹数量
        _HologramStripesSpeed ("Stripes Speed", Range(-20, 20)) = 4.5 //条纹移动速度
        _HologramMinAlpha ("Min Alpha", Range(0, 1)) = 0.1 //最小的alpha值
        _HologramMaxAlpha ("Max Alpha", Range(0, 1)) = 0.75 //最大的alpha值
        _HologramStripeColor ("Stripes Color", Color) = (0, 1, 1, 1) //条纹颜色
        _HologramBlend ("Hologram Blend", Range(0, 1)) = 1 //颜色混合程度

    }
    SubShader {
        Tags { "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            half _HologramStripesAmount, _HologramMinAlpha, _HologramStripesSpeed, _HologramMaxAlpha, _HologramBlend;
            half4 _HologramStripeColor;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            //用于将一个范围内的值映射到另一个范围内
            half RemapFloat(half inValue, half inMin, half inMax, half outMin, half outMax) {
                return outMin + (inValue - inMin) * (outMax - outMin) / (inMax - inMin);
            }

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);

                half totalHologram = _HologramStripesAmount;
                half hologramYCoord = ((i.uv.y + (((_Time.x) % 1) * _HologramStripesSpeed)) % totalHologram) / totalHologram;//计算全息条纹的Y坐标位置
                hologramYCoord = abs(hologramYCoord);//取全息条纹的绝对值，确保Y坐标在正数范围内
                half alpha = RemapFloat(saturate(hologramYCoord), 0.0, 1.0, _HologramMinAlpha, saturate(_HologramMaxAlpha));//根据条纹的Y坐标计算alpha值
                half hologramMask = max(sign(-hologramYCoord), 0.0);//计算一个用于控制全息效果的显示范围的值
                half4 hologramResult = col;
                hologramResult.a *= lerp(alpha, 1, hologramMask);//根据计算的alpha值和掩码值，调整hologramResult的alpha通道值
                hologramResult.rgb *= max(1, _HologramMaxAlpha * max(sign(hologramYCoord), 0.0));//根据条纹的Y坐标位置，调整hologramResult的RGB颜色值
                hologramMask = 1 - step(0.01, hologramMask);//根据掩码值，计算一个反向掩码。如果掩码值小于0.01，则将其设置为1，否则设置为0
                hologramResult.rgb += hologramMask * _HologramStripeColor * col.a;//根据反向掩码，添加全息条纹的颜色
                col = lerp(col, hologramResult, _HologramBlend);//将原始颜色col和经过全息效果处理后的颜色hologramResult进行混合

                return col;
            }
            ENDCG
        }
    }
}