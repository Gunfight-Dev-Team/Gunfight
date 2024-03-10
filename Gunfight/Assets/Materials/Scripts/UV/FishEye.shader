Shader "Hidden/FishEye" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _FishEyeUvAmount ("Fish Eye Amount", Range(0, 0.5)) = 0.35 //���۷Ŵ�ϵ��

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
            half _FishEyeUvAmount;

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
                half2 centerTiled = half2(0.5, 0.5);//������һ���뾶Ϊ0.5�����ĵ�
                half bind = length(centerTiled);//���������ĵ㵽ԭ��ľ��룬�����ĵ��ģ��
                half2 dF = i.uv - centerTiled;//������UV���������ĵ�Ĳ�ֵ����UV��������ĵ��ƫ����
                half dFlen = length(dF);//������ƫ������ģ����������UV��������ĵ�ľ���
                half fishInt = (3.14159265359 / bind) * (_FishEyeUvAmount + 0.001);//����������Ч����ǿ��
                i.uv = centerTiled + (dF / (max(0.0001, dFlen))) * tan(dFlen * fishInt) * bind / tan(bind * fishInt);//����������Ч���ļ���
                fixed4 col = tex2D(_MainTex, i.uv);//����

                return col;
            }
            ENDCG
        }
    }
}