Shader "Hidden/Twist" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _TwistUvAmount ("Twist Amount", Range(0, 3.1416)) = 1
        _TwistUvRadius ("Twist Radius", Range(0, 3)) = 0.75

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
            float4 _MainTex_ST;
            half _TwistUvAmount, _TwistUvRadius;

            fixed4 frag(v2f i) : SV_Target {
                half2 tempUv = i.uv - half2(0.5, 0.5);
                half percent = (_TwistUvRadius - length(tempUv)) / (_TwistUvRadius + 0.001);
                half theta = percent * percent * (2.0 * sin(_TwistUvAmount)) * 8;
                half s = sin(theta);
                half c = cos(theta);
                half beta = max(sign(_TwistUvRadius - length(tempUv)), 0);

                tempUv = half2(dot(tempUv, half2(c, -s)), dot(tempUv, half2(s, c))) * beta;
                tempUv += half2(0.5, 0.5);
                i.uv = tempUv;
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}