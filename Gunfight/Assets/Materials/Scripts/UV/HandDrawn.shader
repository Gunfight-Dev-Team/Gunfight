Shader "Hidden/HandDrawn" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _HandDrawnAmount ("Hand Drawn Amount", Range(0, 20)) = 10 //手绘的偏差的数值
        _HandDrawnSpeed ("Hand Drawn Speed", Range(1, 15)) = 5 //手绘波动的速度

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
            half _HandDrawnAmount, _HandDrawnSpeed;

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
                half2 uvCopy = i.uv;
                _HandDrawnSpeed = floor(_Time * 20 * _HandDrawnSpeed);//将_HandDrawnSpeed与时间关联 要用floor才有一段一段的感觉 不然就是连续波动了
                uvCopy.x = sin((uvCopy.x * _HandDrawnAmount + _HandDrawnSpeed) * 4);//使用sin函数来计算采样周围的sin区域的偏差值
                uvCopy.y = cos((uvCopy.y * _HandDrawnAmount + _HandDrawnSpeed) * 4);//同理
                i.uv = lerp(i.uv, i.uv + uvCopy, 0.0005 * _HandDrawnAmount);//将原始UV坐标与经过处理的UV坐标进行插值 用小数乘为了尽量靠近0 太大了就会轮回一圈采样了
                fixed4 col = tex2D(_MainTex, i.uv);//采样
                return col;
            }
            ENDCG
        }
    }
}