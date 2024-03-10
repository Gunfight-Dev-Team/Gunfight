Shader "Hidden/HandDrawn" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _HandDrawnAmount ("Hand Drawn Amount", Range(0, 20)) = 10
        _HandDrawnSpeed ("Hand Drawn Speed", Range(1, 15)) = 5

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
            half _HandDrawnAmount, _HandDrawnSpeed;

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

            fixed4 frag(v2f i) : SV_Target {
                half2 uvCopy = i.uv;
                _HandDrawnSpeed = floor(_Time * 20 * _HandDrawnSpeed);
                uvCopy.x = sin((uvCopy.x * _HandDrawnAmount + _HandDrawnSpeed) * 4);
                uvCopy.y = cos((uvCopy.y * _HandDrawnAmount + _HandDrawnSpeed) * 4);
                i.uv = lerp(i.uv, i.uv + uvCopy, 0.0005 * _HandDrawnAmount);
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}