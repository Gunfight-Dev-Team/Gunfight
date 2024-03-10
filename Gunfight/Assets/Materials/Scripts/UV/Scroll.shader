Shader "Hidden/Scroll" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _TextureScrollXSpeed ("Speed X Axis", Range(-5, 5)) = 1 //����X���ٶ�
        _TextureScrollYSpeed ("Speed Y Axis", Range(-5, 5)) = 0 //����Y���ٶ�

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
            half _TextureScrollXSpeed, _TextureScrollYSpeed;

            fixed4 frag(v2f i) : SV_Target {
                //ʱ��ı仯��Ӱ��ƫ�����ļ��㣬�Ӷ�ʵ�������������Ч��
                i.uv.x += (_Time.y * _TextureScrollXSpeed) % 1;//����ʱ���_TextureScrollXSpeed��������x�����ϵ�ƫ����
                i.uv.y += (_Time.y * _TextureScrollYSpeed) % 1;//����ʱ���_TextureScrollYSpeed��������y�����ϵ�ƫ����
                fixed4 col = tex2D(_MainTex, i.uv);//����
                return col;
            }
            ENDCG
        }
    }
}