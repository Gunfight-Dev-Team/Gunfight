Shader "Hidden/Ghost" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _GhostColorBoost ("Ghost Color Boost", Range(0, 5)) = 1 //幽灵化后的亮度
        _GhostTransparency ("Ghost Transparency", Range(0, 1)) = 0 //透明度
        _GhostBlend ("Ghost Blend", Range(0, 1)) = 1 // 透明的颜色混合程度

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

                half luminance = 0.3 * col.r + 0.59 * col.g + 0.11 * col.b;//计算了当前像素的亮度（灰度值）常见的计算灰度的公式
                half4 ghostResult;
                ghostResult.a = saturate(luminance - _GhostTransparency);//计算了“幽灵效果”的透明度 从像素的亮度中减去_GhostTransparency 参数
                //计算了“幽灵效果”的颜色。它将原始颜色与像素的亮度加上_GhostColorBoost参数的乘积相乘，这样可以根据像素的亮度调整幽灵效果的颜色。
                ghostResult.rgb = col.rgb * (luminance + _GhostColorBoost);
                col = lerp(col, ghostResult, _GhostBlend);//将原始颜色和幽灵效果的颜色进行混合。混合比例由_GhostBlend参数控制

                return col;
            }
            ENDCG
        }
    }
}