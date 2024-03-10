Shader "Hidden/Zoom" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _ZoomUvAmount ("Zoom Amount", Range(0.1, 5)) = 0.5 //108

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
            half _ZoomUvAmount;

            fixed4 frag(v2f i) : SV_Target {
                half2 centerTiled = half2(0.5, 0.5);//定义了一个半径为0.5的中心点
                i.uv -= centerTiled;//将纹理坐标平移到以纹理中心为原点的坐标系中
                i.uv = i.uv * _ZoomUvAmount;//对纹理坐标进行缩放操作
                i.uv += centerTiled;//将缩放后的纹理坐标平移到原来的坐标系中心
                fixed4 col = tex2D(_MainTex, i.uv);//采样
                return col;
            }
            ENDCG
        }
    }
}