Shader "Hidden/Twist" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _PinchUvAmount ("Pinch Amount", Range(0, 0.5)) = 0.35 //���ǿ��ϵ��

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
                half2 dP = i.uv - centerTiled;//�����˵�ǰ�������������ĵ��������
                //length(centerTiled)�������������ĵ㵽ԭ��ľ���
                //�æг���������룬�õ�һ������ֵ�����ڿ���ͼ�������ĳ̶�
                //(-_PinchUvAmount + 0.001)��һ���û�ָ���Ĳ��������ڵ���������ǿ��
                half pinchInt = (3.141592 / length(centerTiled)) * (-_PinchUvAmount + 0.001);
                //normalize(dP)������dP��һ������ȷ������ֻ�Է�����в��������ı��䳤��
                //atan(length(dP)*-pinchInt*10.0)������ÿ�����ص�ĽǶ�ƫ�ƣ�����Ƕ�ƫ���ɾ������ĵ�ľ��������ǿ��pinchInt���� atan���������ڽ�����ת��Ϊ�Ƕ�
                //* 0.5 / atan(-pinchInt * 5)ͨ��������һ��arctan�����Ľ��������������Ƕ�ƫ�ƣ���ȷ�����������ĵ㴦û�б仯
                i.uv = centerTiled + normalize(dP) * atan(length(dP) * - pinchInt * 10.0) * 0.5 / atan(-pinchInt * 5);
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
