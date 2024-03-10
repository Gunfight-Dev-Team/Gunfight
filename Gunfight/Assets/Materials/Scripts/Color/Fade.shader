Shader "Hidden/Fade" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }

        _FadeTex ("FadeTexture", 2D) = "white" { }
        _FadeAmount ("FadeAmount", Range(0, 1)) = 0
        _FadeColor ("FadeColor", Color) = (1, 1, 0, 1)
        _FadeBurnWidth ("Fade Burn Width", Range(0, 1)) = 0.02

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
            sampler2D _FadeTex;
            float4 _FadeTex_ST;
            float _FadeAmount, _FadeBurnWidth;
            fixed4 _FadeColor;

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
                fixed4 col = tex2D(_MainTex, i.uv);
                float originalAlpha = col.a;

                float2 tiledUvFade = TRANSFORM_TEX(i.uv, _FadeTex);

                float fadeTemp = tex2D(_FadeTex, tiledUvFade).r;
                float fade = step(_FadeAmount, fadeTemp);
                float fadeBurn = saturate(step(_FadeAmount - _FadeBurnWidth, fadeTemp));
                col.a *= fade;
                col += fadeBurn * tex2D(_FadeTex, tiledUvFade) * _FadeColor * originalAlpha * (1 - col.a);

                return col;
            }
            ENDCG
        }
    }
}