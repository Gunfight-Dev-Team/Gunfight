Shader "Hidden/InnerOutline" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _InnerOutlineColor ("InnerOutlineColor", Color) = (1, 0, 0, 1) //���������ɫ
        _InnerOutlineThickness ("OutlineThickness", Int) = 2 //���������ƫ��ֵ���ж����ش�С
        _InnerOutlineAlpha ("InnerOutlineAlpha", Range(0, 1)) = 1 //�������͸����
        _InnerOutlineGlow ("InnerOutlineGlow", Range(1, 10)) = 4 //�����������̶�

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
            float2 _MainTex_TexelSize;
            float _InnerOutlineThickness, _InnerOutlineAlpha, _InnerOutlineGlow;
            fixed4 _InnerOutlineColor;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            //�������������þ��Ǹ��ݴ������������ uv ��ƫ�������������Ͻ��в�������ȡ��Ӧλ�õ�������ɫ��
            float3 GetPixel(in int offsetX, in int offsetY, half2 uv, sampler2D tex) {
                return tex2D(tex, (uv + half2(offsetX * _MainTex_TexelSize.x, offsetY * _MainTex_TexelSize.y))).rgb;
            }

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);

                //��x��y��ֱ�ȡ��ƫ��ֵ ��ȡ����������������ˮƽ�ʹ�ֱ�����ϵ���ɫ����ֵ
                float3 innerT = abs(GetPixel(0, _InnerOutlineThickness, i.uv, _MainTex) - GetPixel(0, -_InnerOutlineThickness, i.uv, _MainTex));
                innerT += abs(GetPixel(_InnerOutlineThickness, 0, i.uv, _MainTex) - GetPixel(-_InnerOutlineThickness, 0, i.uv, _MainTex));

                innerT = innerT * col.a * _InnerOutlineAlpha;//����alphaֵ
                //��innerTȡģ���ٽ�����ɫ��� ��ΪinnerT�ǵ�ǰƬԪ������ƬԪ����ɫ����ֵ��Ҫȡģ���ܱ�ʾ����Ĵ�С
                col.rgb += length(innerT) * _InnerOutlineColor.rgb * _InnerOutlineGlow;

                return col;
            }
            ENDCG
        }
    }
}