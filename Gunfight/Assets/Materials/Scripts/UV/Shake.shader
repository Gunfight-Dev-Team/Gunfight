Shader "Hidden/Shake" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _ShakeUvSpeed ("Shake Speed", Range(0, 20)) = 2.5 //抖动速度
        _ShakeUvX ("X Multiplier", Range(0, 5)) = 1.5 //x轴抖动幅度
        _ShakeUvY ("Y Multiplier", Range(0, 5)) = 1 //y轴抖动幅度

    }
    SubShader {
        Tags { "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            half _ShakeUvSpeed, _ShakeUvX, _ShakeUvY;

            fixed4 frag(v2f i) : SV_Target {
                half xShake = sin(_Time * _ShakeUvSpeed * 50) * _ShakeUvX;//根据时间，速度和振幅计算了在x方向上的振动量。使用正弦函数来产生周期性的振动效果
                half yShake = cos(_Time * _ShakeUvSpeed * 50) * _ShakeUvY;//根据时间，速度和振幅计算了在y方向上的振动量。使用余弦函数来产生周期性的振动效果
                i.uv += half2(xShake * 0.01, yShake * 0.01);//将计算得到的振动量应用到当前像素的纹理坐标上
                fixed4 col = tex2D(_MainTex, i.uv);//采样
                return col;
            }
            ENDCG
        }
    }
}