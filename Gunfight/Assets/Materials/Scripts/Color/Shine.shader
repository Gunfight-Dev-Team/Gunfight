Shader "Hidden/Shine" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _ShineColor ("Shine Color", Color) = (1, 1, 1, 1) //光线的颜色
        _ShineLocation ("Shine Location", Range(0, 1)) = 0.5 //光线的位置
        _ShineRotate ("Rotate Angle(radians)", Range(0, 6.2831)) = 0 //2Π 360度
        _ShineWidth ("Shine Width", Range(0.05, 1)) = 0.1 //光线宽度
        _ShineGlow ("Shine Glow", Range(0, 100)) = 1 //光线亮度

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
				half cosAngle = cos(_ShineRotate);//分别计算光线旋转角度_ShineRotate的余弦值
				half sinAngle = sin(_ShineRotate);//分别计算光线旋转角度_ShineRotate的正弦值
				half2x2 rot = half2x2(cosAngle, -sinAngle, sinAngle, cosAngle);//创建一个二维旋转矩阵rot来旋转纹理坐标
				uvShine -= half2(0.5, 0.5);//将纹理坐标进行平移，使其以纹理的中心为原点 要让(0.5,0.5)的地方变成(0,0)
				uvShine = mul(rot, uvShine);//将纹理坐标按照旋转矩阵rot进行旋转
				uvShine += half2(0.5, 0.5);//将旋转后的纹理坐标恢复到原来的位置
				half currentDistanceProjection = (uvShine.x + uvShine.y) / 2;//计算当前像素在光线投影方向上的距离投影
				half whitePower = 1 - (abs(currentDistanceProjection - _ShineLocation) / _ShineWidth);//计算当前像素的亮度，用于模拟光线的强度
				col.rgb +=  col.a * whitePower * _ShineGlow * max(sign(currentDistanceProjection - (_ShineLocation - _ShineWidth)), 0.0)
				* max(sign((_ShineLocation + _ShineWidth) - currentDistanceProjection), 0.0) * _ShineColor;//计算出最终的颜色

                return col;
            }
            ENDCG
        }
    }
}