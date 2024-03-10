Shader "Hidden/Shake" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _ShakeUvSpeed ("Shake Speed", Range(0, 20)) = 2.5
        _ShakeUvX ("X Multiplier", Range(0, 5)) = 1.5
        _ShakeUvY ("Y Multiplier", Range(0, 5)) = 1

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
                half xShake = sin(_Time * _ShakeUvSpeed * 50) * _ShakeUvX;
                half yShake = cos(_Time * _ShakeUvSpeed * 50) * _ShakeUvY;
                i.uv += half2(xShake * 0.01, yShake * 0.01);
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}