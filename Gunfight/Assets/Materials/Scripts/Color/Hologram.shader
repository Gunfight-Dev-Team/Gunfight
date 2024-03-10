Shader "Hidden/Hologram" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _HologramStripesAmount ("Stripes Amount", Range(0, 1)) = 0.1
        _HologramStripesSpeed ("Stripes Speed", Range(-20, 20)) = 4.5
        _HologramMinAlpha ("Min Alpha", Range(0, 1)) = 0.1
        _HologramMaxAlpha ("Max Alpha", Range(0, 1)) = 0.75
        _HologramStripeColor ("Stripes Color", Color) = (0, 1, 1, 1) 
        _HologramBlend ("Hologram Blend", Range(0, 1)) = 1 

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
                half hologramYCoord = ((i.uv.y + (((_Time.x) % 1) * _HologramStripesSpeed)) % totalHologram) / totalHologram;
                hologramYCoord = abs(hologramYCoord);
                half alpha = RemapFloat(saturate(hologramYCoord), 0.0, 1.0, _HologramMinAlpha, saturate(_HologramMaxAlpha));
                half hologramMask = max(sign(-hologramYCoord), 0.0);
                half4 hologramResult = col;
                hologramResult.a *= lerp(alpha, 1, hologramMask);
                hologramResult.rgb *= max(1, _HologramMaxAlpha * max(sign(hologramYCoord), 0.0));
                hologramMask = 1 - step(0.01, hologramMask);
                hologramResult.rgb += hologramMask * _HologramStripeColor * col.a;
                col = lerp(col, hologramResult, _HologramBlend);

                return col;
            }
            ENDCG
        }
    }
}