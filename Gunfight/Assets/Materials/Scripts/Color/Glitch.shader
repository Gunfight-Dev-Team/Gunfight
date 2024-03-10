Shader "Hidden/Glitch" {
    //添加一些视觉上的“故障”效果
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _GlitchAmount ("Glitch Amount", Range(0, 20)) = 3 //偏差的移动范围大小
        _GlitchSize ("Glitch Size", Range(0.25, 5)) = 1 //偏差的颗粒大小

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
            half _GlitchAmount, _GlitchSize;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            //生成伪随机数
            half rand2(half2 seed, half offset) {
                //50 + (_Time % 1.0) * 12根据时间生成一个随机的float型变量
                //点乘种子向量和half2(127.1, 311.7)得到一个新的向量
                //将点乘结果传入正弦函数中，以增加随机性
                //frac用于获取浮点数的小数部分的函数 中间乘以一个很大的无规律的数再加上偏移量用于调整随机数的范围
                //将结果取余1.0，确保随机数在[0, 1)范围内
                return (frac(sin(dot(seed * floor(50 + (_Time % 1.0) * 12), half2(127.1, 311.7))) * 43758.5453123) + offset) % 1.0;
            }

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                //_GlitchSize 是一个参数，用于调整纹理坐标的缩放比例。
                //将纹理坐标乘以half2(24, 19)将纹理坐标的范围缩放到合适的范围
                //然后乘以_GlitchSize，再取floor操作。最终乘以4，以增加噪声的频率
                //这样得到的是一个随机数的种子，用于产生噪声
                //用rand2生成了一个随机数
                //生成的随机数进行了立方运算，这将增加噪声的强度，使其更加明显
                //第二组操作与第一组操作类似，只是使用了不同的参数。第二组操作用于生成另一组噪声。两组噪声被乘在一起，产生更加复杂的效果
                //lineNoise 变量存储了两组噪声的乘积，它将用于扰动纹理坐标
                half lineNoise = pow(rand2(floor(i.uv * half2(24, 19) * _GlitchSize) * 4, 1), 3.0) * _GlitchAmount
                * pow(rand2(floor(i.uv * half2(38, 14) * _GlitchSize) * 4, 1), 3.0);
                //将上述生成的两组随机噪声应用到当前像素的纹理坐标上，从而产生了横向的扰动。再进行采样
                col = tex2D(_MainTex, i.uv + half2(lineNoise * 0.02 * rand2(half2(2.0, 1), 1), 0));
                
                return col;
            }
            ENDCG
        }
    }
}