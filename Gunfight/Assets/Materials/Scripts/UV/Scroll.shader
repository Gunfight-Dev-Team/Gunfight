Shader "Hidden/Scroll" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _TextureScrollXSpeed ("Speed X Axis", Range(-5, 5)) = 1
        _TextureScrollYSpeed ("Speed Y Axis", Range(-5, 5)) = 0

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
            half _TextureScrollXSpeed, _TextureScrollYSpeed;

            fixed4 frag(v2f i) : SV_Target {
              
                i.uv.x += (_Time.y * _TextureScrollXSpeed) % 1;
                i.uv.y += (_Time.y * _TextureScrollYSpeed) % 1;
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}