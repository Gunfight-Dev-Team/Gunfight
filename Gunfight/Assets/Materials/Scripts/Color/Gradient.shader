Shader "Hidden/Gradient" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _GradBlend ("GradientBlend", Range(0, 1)) = 1
        _GradTopLeftCol ("TopLeftCol", Color) = (1, 0, 0, 1)
        _GradTopRightCol ("TopRightColor", Color) = (1, 1, 0, 1)
        _GradBottomLeftColor ("BottomLeftColor", Color) = (0, 0, 1, 1)
        _GradBottomRightColor ("BottomRightColor", Color) = (0, 1, 0, 1)
        _GradBoostX ("GradBoostX", Range(0.1, 2)) = 1.2
        _GradBoostY ("_GradBoostY", Range(0.1, 2)) = 1.2

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
            float2 _MainTex_ST;
            float _GradBlend, _GradBoostX, _GradBoostY;
            fixed4 _GradTopRightCol, _GradTopLeftCol, _GradBotRightCol, _GradBotLeftCol;

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

                float gradXLerpFactor = saturate(pow(i.uv.x, _GradBoostX));
                float gradYLerpFactor = saturate(pow(i.uv.y, _GradBoostY));
                fixed4 gradientResult = lerp(lerp(_GradBotLeftCol, _GradBotRightCol, gradXLerpFactor),
                lerp(_GradTopLeftCol, _GradTopRightCol, gradXLerpFactor), gradYLerpFactor);
                gradientResult = lerp(col, gradientResult, _GradBlend);
                col.rgb = gradientResult.rgb * col.a;
                return col;
            }
            ENDCG
        }
    }
}