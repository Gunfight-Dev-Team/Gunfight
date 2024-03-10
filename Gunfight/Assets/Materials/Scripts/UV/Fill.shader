Shader "Hidden/Fill" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _ClipUvLeft ("Clipping Left", Range(0, 1)) = 0 //102
        _ClipUvRight ("Clipping Right", Range(0, 1)) = 0 //103
        _ClipUvUp ("Clipping Up", Range(0, 1)) = 0 //104
        _ClipUvDown ("Clipping Down", Range(0, 1)) = 0 //105

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
            float4 _MainTex_ST;
            half _ClipUvLeft, _ClipUvRight, _ClipUvUp, _ClipUvDown;

            fixed4 frag(v2f i) : SV_Target {
                half2 tiledUv = half2(i.uv.x / _MainTex_ST.x, i.uv.y / _MainTex_ST.y);
                //ͨ�� (1 - _ClipUvUp) ������ü��Ķ���λ�ã�Ȼ���� tiledUv.y �Ƚϣ����������Χ���������ü�
                clip((1 - _ClipUvUp) - tiledUv.y);
                //ʹ�� _ClipUvDown ���Ʋü��ĵײ�λ��
                clip(tiledUv.y - _ClipUvDown);
                //�ü���������Ҳ��֣�ʹ�� _ClipUvRight ���Ʋü����Ҳ�λ��
                clip((1 - _ClipUvRight) - tiledUv.x);
                //�ü���������󲿷֣�ʹ�� _ClipUvLeft ���Ʋü������λ��
                clip(tiledUv.x - _ClipUvLeft);
                fixed4 col = tex2D(_MainTex, i.uv);//����

                return col;
            }
            ENDCG
        }
    }
}