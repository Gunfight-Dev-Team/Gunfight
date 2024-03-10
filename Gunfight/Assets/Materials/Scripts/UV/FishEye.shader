Shader "Hidden/FishEye" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _FishEyeUvAmount ("Fish Eye Amount", Range(0, 0.5)) = 0.35 //鱼眼放大系数

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
                half2 centerTiled = half2(0.5, 0.5);//定义了一个半径为0.5的中心点
                half bind = length(centerTiled);//计算了中心点到原点的距离，即中心点的模长
                half2 dF = i.uv - centerTiled;//计算了UV坐标与中心点的差值，即UV相对于中心点的偏移量
                half dFlen = length(dF);//计算了偏移量的模长，即输入UV相对于中心点的距离
                half fishInt = (3.14159265359 / bind) * (_FishEyeUvAmount + 0.001);//计算了鱼眼效果的强度
                i.uv = centerTiled + (dF / (max(0.0001, dFlen))) * tan(dFlen * fishInt) * bind / tan(bind * fishInt);//进行了鱼眼效果的计算
                fixed4 col = tex2D(_MainTex, i.uv);//采样

                return col;
            }
            ENDCG
        }
    }
}