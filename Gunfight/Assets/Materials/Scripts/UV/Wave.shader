Shader "Hidden/Wave" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _WaveAmount ("Wave Amount", Range(0, 25)) = 7 //������ƫ��ֵ
        _WaveSpeed ("Wave Speed", Range(0, 25)) = 10 //�����ٶ�
        _WaveStrength ("Wave Strength", Range(0, 25)) = 7.5 //�����ķ��ȴ�С
        _WaveX ("Wave X Axis", Range(0, 1)) = 0 //������ԭ��X
        _WaveY ("Wave Y Axis", Range(0, 1)) = 0.5 //������ԭ��Y

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
                float2 uvWave = half2(_WaveX *  _MainTex_ST.x, _WaveY *  _MainTex_ST.y) - i.uv;//�õ�һ������ڵ�ǰ����λ�õĲ�������
                uvWave.x *= _ScreenParams.x / _ScreenParams.y;//���������һ����Ļ��߱ȵ����ӣ�Ŀ�����ڷǷ�����Ļ�ϱ��ֲ��˵ı���
            	float waveTime = _Time.y;
                //ʹ�� sqrt(dot(uvWave, uvWave)) ���Եõ������ĳ��ȣ���������ģ
                //���ǽ����˵�ǿ�Ȳ���_WaveAmount���Բ��˵ĳ��ȣ��Ե������˵�����ǿ��
                //�����������ǴӲ��˵�ǿ���м�ȥ��һ������ʱ�������
				float angWave = (sqrt(dot(uvWave, uvWave)) * _WaveAmount) - ((waveTime *  _WaveSpeed));//���㲨�˵ĽǶ�
                //��һ�д��뽫��ǰ���ص���������i.uvƫ����һ��������ƫ�����ɲ�������uvWave���Ƕ�angWave�Ͳ���ǿ��_WaveStrength����
				i.uv = i.uv + uvWave * sin(angWave) * (_WaveStrength / 1000.0);//ʹ�����Һ�������������Ч��
                fixed4 col = tex2D(_MainTex, i.uv);//����

                return col;
            }
            ENDCG
        }
    }
}