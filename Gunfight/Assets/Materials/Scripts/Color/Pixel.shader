Shader "Hidden/Pixel" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _PixelateSize ("Pixelate size", Range(4, 512)) = 32 //像素大小

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
            float _PixelateSize;//假设_PixelateSize的值为2，意味着每个纹理坐标会被放大两倍

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                //将当前像素的纹理坐标进行放大 如果原始的纹理坐标是(0.3, 0.7),而 _PixelateSize是2，那么放大后的纹理坐标就会变成(0.6, 1.4)。
                //round()函数将放大后的纹理坐标四舍五入取整。如果放大后的纹理坐标是(0.6, 1.4)，那么经过取整后就会变成(1, 1)。再次缩小_PixelateSize 完成像素化
                i.uv = round(i.uv * _PixelateSize) / _PixelateSize;
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;//重新采样
                clip(col.a - 0.51);//类似discard 在alpha小于0.51的时候这个像素给剔除
                return col;
            }
            ENDCG
        }
    }
}