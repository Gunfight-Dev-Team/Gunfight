Shader "Hidden/Ghost" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _GhostColorBoost ("Ghost Color Boost", Range(0, 5)) = 1
        _GhostTransparency ("Ghost Transparency", Range(0, 1)) = 0 
        _GhostBlend ("Ghost Blend", Range(0, 1)) = 1 

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
            half _GhostColorBoost, _GhostTransparency, _GhostBlend;

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

                half luminance = 0.3 * col.r + 0.59 * col.g + 0.11 * col.b;
                half4 ghostResult;
                ghostResult.a = saturate(luminance - _GhostTransparency);
                ghostResult.rgb = col.rgb * (luminance + _GhostColorBoost);
                col = lerp(col, ghostResult, _GhostBlend);

                return col;
            }
            ENDCG
        }
    }
}