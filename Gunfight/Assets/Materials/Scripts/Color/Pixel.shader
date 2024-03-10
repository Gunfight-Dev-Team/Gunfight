Shader "Hidden/Pixel" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _PixelateSize ("Pixelate size", Range(4, 512)) = 32

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
            float _PixelateSize;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
             
                i.uv = round(i.uv * _PixelateSize) / _PixelateSize;
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                clip(col.a - 0.51);
                return col;
            }
            ENDCG
        }
    }
}