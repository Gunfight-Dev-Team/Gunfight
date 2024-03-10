Shader "Hidden/InnerOutline" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _InnerOutlineColor ("InnerOutlineColor", Color) = (1, 0, 0, 1) //描边线条颜色
        _InnerOutlineThickness ("OutlineThickness", Int) = 2 //描边线条的偏差值的判定像素大小
        _InnerOutlineAlpha ("InnerOutlineAlpha", Range(0, 1)) = 1 //描边线条透明度
        _InnerOutlineGlow ("InnerOutlineGlow", Range(1, 10)) = 4 //描边线条发光程度

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

            //整个函数的作用就是根据传入的纹理坐标 uv 和偏移量，在纹理上进行采样，获取对应位置的像素颜色。
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

                //对x和y轴分别取样偏移值 获取内轮廓的两个方向（水平和垂直方向）上的颜色差异值
                float3 innerT = abs(GetPixel(0, _InnerOutlineThickness, i.uv, _MainTex) - GetPixel(0, -_InnerOutlineThickness, i.uv, _MainTex));
                innerT += abs(GetPixel(_InnerOutlineThickness, 0, i.uv, _MainTex) - GetPixel(-_InnerOutlineThickness, 0, i.uv, _MainTex));

                innerT = innerT * col.a * _InnerOutlineAlpha;//控制alpha值
                //对innerT取模长再进行颜色相乘 因为innerT是当前片元与相邻片元的颜色差异值，要取模才能表示差异的大小
                col.rgb += length(innerT) * _InnerOutlineColor.rgb * _InnerOutlineGlow;

                return col;
            }
            ENDCG
        }
    }
}