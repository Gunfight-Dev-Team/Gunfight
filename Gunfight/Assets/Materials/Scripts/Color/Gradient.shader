Shader "Hidden/Gradient" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _GradBlend ("GradientBlend", Range(0, 1)) = 1 //颜色混合的程度
        _GradTopLeftCol ("TopLeftCol", Color) = (1, 0, 0, 1) //左上角的颜色
        _GradTopRightCol ("TopRightColor", Color) = (1, 1, 0, 1) //右上角的颜色
        _GradBottomLeftColor ("BottomLeftColor", Color) = (0, 0, 1, 1) //左下角的颜色
        _GradBottomRightColor ("BottomRightColor", Color) = (0, 1, 0, 1) //右下角的颜色
        _GradBoostX ("GradBoostX", Range(0.1, 2)) = 1.2 //左边和右边的占比
        _GradBoostY ("_GradBoostY", Range(0.1, 2)) = 1.2 //上边和下边的占比

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

                float gradXLerpFactor = saturate(pow(i.uv.x, _GradBoostX));//水平梯度的平滑因子 用pow计算，我也不知道为什么
                float gradYLerpFactor = saturate(pow(i.uv.y, _GradBoostY));//垂直梯度的平滑因子
                //根据水平和垂直方向的插值因子,以及颜色梯度的四个角色彩颜色,值通过双线性插值计算出最终的颜色梯度效果。
                fixed4 gradientResult = lerp(lerp(_GradBotLeftCol, _GradBotRightCol, gradXLerpFactor),
                lerp(_GradTopLeftCol, _GradTopRightCol, gradXLerpFactor), gradYLerpFactor);
                gradientResult = lerp(col, gradientResult, _GradBlend);//将颜色梯度效果与原始纹理颜色进行混合，根据_GradBlend的值进行插值。
                col.rgb = gradientResult.rgb * col.a;//将混合后的颜色应用到原始颜色的RGB分量上，同时乘以原始颜色的透明度，以确保颜色混合后的透明度正确
                return col;
            }
            ENDCG
        }
    }
}