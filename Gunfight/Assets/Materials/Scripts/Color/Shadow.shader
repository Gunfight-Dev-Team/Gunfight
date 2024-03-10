Shader "Hidden/Shadow" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _ShadowX ("Shadow X Axis", Range(-0.5, 0.5)) = 0.1 //x轴偏移值
        _ShadowY ("Shadow Y Axis", Range(-0.5, 0.5)) = -0.05 //y轴偏移值
        _ShadowAlpha ("Shadow Alpha", Range(0, 1)) = 0.5 //影子alpha值

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
            half _ShadowX, _ShadowY, _ShadowAlpha;
            half4 _ShadowColor;

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

            //通过调整像素的alpha值，使得_MainTex纹理上的一部分区域看起来更加暗淡
            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                half shadowA = tex2D(_MainTex, i.uv + half2(_ShadowX, _ShadowY)).a;//用偏移过的uv进行采样 并且记录alpha值 可以理解为影子的alpha
                col.a = max(shadowA * _ShadowAlpha, col.a);//设置了当前像素的alpha值 取最大值是为了确保阴影的alpha值不会低于片元颜色的alpha值。
                return col;
            }
            ENDCG
        }
    }
}