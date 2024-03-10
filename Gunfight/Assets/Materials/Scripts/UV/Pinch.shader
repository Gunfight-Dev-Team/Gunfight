Shader "Hidden/Twist" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _PinchUvAmount ("Pinch Amount", Range(0, 0.5)) = 0.35

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
            half _PinchUvAmount;

            fixed4 frag(v2f i) : SV_Target {
                half2 centerTiled = half2(0.5,0.5);
                half2 dP = i.uv - centerTiled;
                half pinchInt = (3.141592 / length(centerTiled)) * (-_PinchUvAmount + 0.001);
              
                i.uv = centerTiled + normalize(dP) * atan(length(dP) * - pinchInt * 10.0) * 0.5 / atan(-pinchInt * 5);
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
