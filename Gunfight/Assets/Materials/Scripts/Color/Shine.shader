Shader "Hidden/Shine" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _ShineColor ("Shine Color", Color) = (1, 1, 1, 1)
        _ShineLocation ("Shine Location", Range(0, 1)) = 0.5 
        _ShineRotate ("Rotate Angle(radians)", Range(0, 6.2831)) = 0 
        _ShineWidth ("Shine Width", Range(0.05, 1)) = 0.1
        _ShineGlow ("Shine Glow", Range(0, 100)) = 1

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
            half4 _ShineColor;
            half _ShineLocation, _ShineRotate, _ShineWidth, _ShineGlow;

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
                
                half2 uvShine = i.uv;
				half cosAngle = cos(_ShineRotate);
				half sinAngle = sin(_ShineRotate);
				half2x2 rot = half2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
				uvShine -= half2(0.5, 0.5);
				uvShine = mul(rot, uvShine);
				uvShine += half2(0.5, 0.5);
				half currentDistanceProjection = (uvShine.x + uvShine.y) / 2;
				half whitePower = 1 - (abs(currentDistanceProjection - _ShineLocation) / _ShineWidth);
				col.rgb +=  col.a * whitePower * _ShineGlow * max(sign(currentDistanceProjection - (_ShineLocation - _ShineWidth)), 0.0)
				* max(sign((_ShineLocation + _ShineWidth) - currentDistanceProjection), 0.0) * _ShineColor;

                return col;
            }
            ENDCG
        }
    }
}