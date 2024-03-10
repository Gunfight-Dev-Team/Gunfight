Shader "Hidden/Twist" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _PinchUvAmount ("Pinch Amount", Range(0, 0.5)) = 0.35 //广角强度系数

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
                half2 dP = i.uv - centerTiled;//计算了当前像素与纹理中心点的向量差
                //length(centerTiled)计算了纹理中心点到原点的距离
                //用π除以这个距离，得到一个比例值，用于控制图像收缩的程度
                //(-_PinchUvAmount + 0.001)是一个用户指定的参数，用于调整收缩的强度
                half pinchInt = (3.141592 / length(centerTiled)) * (-_PinchUvAmount + 0.001);
                //normalize(dP)将向量dP归一化，以确保我们只对方向进行操作，不改变其长度
                //atan(length(dP)*-pinchInt*10.0)计算了每个像素点的角度偏移，这个角度偏移由距离中心点的距离和收缩强度pinchInt决定 atan函数被用于将距离转换为角度
                //* 0.5 / atan(-pinchInt * 5)通过除以另一个arctan函数的结果，来缩放这个角度偏移，以确保在纹理中心点处没有变化
                i.uv = centerTiled + normalize(dP) * atan(length(dP) * - pinchInt * 10.0) * 0.5 / atan(-pinchInt * 5);
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
