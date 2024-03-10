Shader "Hidden/Blur" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _BlurIntensity ("Blur Intensity", Range(0, 12)) = 10

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
            half _BlurIntensity;

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

            half BlurHD_G(half bhqp, half x) {
                return exp( - (x * x) / (2.0 * bhqp * bhqp));
            }

            half4 BlurHD(half2 uv, sampler2D source, half Intensity, half xScale, half yScale) {
                int iterations = 16;
                int halfIterations = iterations / 2;
                half sigmaX = 0.1 + Intensity * 0.5;
                half sigmaY = sigmaX;
                half total = 0.0;
                half4 ret = half4(0, 0, 0, 0);
                for (int iy = 0; iy < iterations; ++iy) {
                    half fy = BlurHD_G(sigmaY, half(iy) - half(halfIterations));
                    half offsetY = half(iy - halfIterations) * 0.00390625 * xScale;
                    for (int ix = 0; ix < iterations; ++ix) {
                        half fx = BlurHD_G(sigmaX, half(ix) - half(halfIterations));
                        half offsetX = half(ix - halfIterations) * 0.00390625 * yScale;
                        total += fx * fy;
                        ret += tex2D(source, uv + half2(offsetX, offsetY)) * fx * fy;
                    }
                }
                return ret / total;
            }

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                col = BlurHD(i.uv, _MainTex, _BlurIntensity, 1, 1) * i.color;
                return col;
            }
            ENDCG
        }
    }
}