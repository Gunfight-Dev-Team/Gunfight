Shader "Hidden/TikTok" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _TikTokAmount ("TikTok Amount", Range(0, 1)) = 0.5
        _TikTokAlpha ("TikTok Alpha", Range(0, 1)) = 0.25

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
            half _TikTokAmount, _TikTokAlpha;

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
                half4 r = tex2D(_MainTex, i.uv + half2(_TikTokAmount / 10, 0));
                half4 b = tex2D(_MainTex, i.uv + half2(-_TikTokAmount / 10, 0));
                col = half4(r.r, col.g, b.b, max(max(r.a, b.a) * _TikTokAlpha, col.a));
                return col;
            }
            ENDCG
        }
    }
}