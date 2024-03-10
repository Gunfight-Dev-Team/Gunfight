Shader "Hidden/Shadow" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _ShadowX ("Shadow X Axis", Range(-0.5, 0.5)) = 0.1
        _ShadowY ("Shadow Y Axis", Range(-0.5, 0.5)) = -0.05
        _ShadowAlpha ("Shadow Alpha", Range(0, 1)) = 0.5

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
            half _ShadowX, _ShadowY, _ShadowAlpha;
            half4 _ShadowColor;

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
                half shadowA = tex2D(_MainTex, i.uv + half2(_ShadowX, _ShadowY)).a;
                col.a = max(shadowA * _ShadowAlpha, col.a);
                return col;
            }
            ENDCG
        }
    }
}