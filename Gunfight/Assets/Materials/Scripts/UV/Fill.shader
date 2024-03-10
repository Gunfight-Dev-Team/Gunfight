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
                //通过 (1 - _ClipUvUp) 计算出裁剪的顶部位置，然后与 tiledUv.y 比较，超出这个范围的纹理将被裁剪
                clip((1 - _ClipUvUp) - tiledUv.y);
                //使用 _ClipUvDown 控制裁剪的底部位置
                clip(tiledUv.y - _ClipUvDown);
                //裁剪了纹理的右部分，使用 _ClipUvRight 控制裁剪的右侧位置
                clip((1 - _ClipUvRight) - tiledUv.x);
                //裁剪了纹理的左部分，使用 _ClipUvLeft 控制裁剪的左侧位置
                clip(tiledUv.x - _ClipUvLeft);
                fixed4 col = tex2D(_MainTex, i.uv);//采样

                return col;
            }
            ENDCG
        }
    }
}