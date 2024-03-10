Shader "Hidden/Wave" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _WaveAmount ("Wave Amount", Range(0, 25)) = 7 //采样的偏差值
        _WaveSpeed ("Wave Speed", Range(0, 25)) = 10 //波动速度
        _WaveStrength ("Wave Strength", Range(0, 25)) = 7.5 //波动的幅度大小
        _WaveX ("Wave X Axis", Range(0, 1)) = 0 //波动的原点X
        _WaveY ("Wave Y Axis", Range(0, 1)) = 0.5 //波动的原点Y

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
            float4 _MainTex_ST;
            float _WaveAmount, _WaveSpeed, _WaveStrength, _WaveX, _WaveY;

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
                float2 uvWave = half2(_WaveX *  _MainTex_ST.x, _WaveY *  _MainTex_ST.y) - i.uv;//得到一个相对于当前像素位置的波浪向量
                uvWave.x *= _ScreenParams.x / _ScreenParams.y;//这里乘以了一个屏幕宽高比的因子，目的是在非方形屏幕上保持波浪的比例
            	float waveTime = _Time.y;
                //使用 sqrt(dot(uvWave, uvWave)) 可以得到向量的长度，即向量的模
                //我们将波浪的强度参数_WaveAmount乘以波浪的长度，以调整波浪的整体强度
                //接下来，我们从波浪的强度中减去了一个基于时间的因素
				float angWave = (sqrt(dot(uvWave, uvWave)) * _WaveAmount) - ((waveTime *  _WaveSpeed));//计算波浪的角度
                //这一行代码将当前像素的纹理坐标i.uv偏移了一定的量，偏移量由波浪向量uvWave、角度angWave和波浪强度_WaveStrength决定
				i.uv = i.uv + uvWave * sin(angWave) * (_WaveStrength / 1000.0);//使用正弦函数来创建波浪效果
                fixed4 col = tex2D(_MainTex, i.uv);//采样

                return col;
            }
            ENDCG
        }
    }
}