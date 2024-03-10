Shader "Hidden/Glow" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1) //全身发光的颜色
        _GlowIntensity ("GlowIntensity", Range(0, 10)) = 2 //纹理或者颜色运用部位的发光的强度
        [NoScaleOffset] _GlowTex ("GlowTexture", 2D) = "white" { }//发光纹理

        _DistortTex ("DistortionTex", 2D) = "white" { }//发光纹理扭曲的噪声图
        _DistortAmount ("DistortionAmount", Range(0, 2)) = 2 //噪声图波动的大小系数
        _DistortTexXSpeed ("DistortTexXSpeed", Range(-50, 50)) = 0 //噪声图波动的X轴速度
        _DistortTexYSpeed ("DistortTexYSpeed", Range(-50, 50)) = -5 //噪声图波动的Y轴速度

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
            sampler2D _GlowTex;
            fixed4 _GlowColor;
            float _GlowIntensity;

            sampler2D _DistortTex;
            float4 _DistortTex_ST;
            float _DistortTexXSpeed, _DistortTexYSpeed, _DistortAmount;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 uvOutDistTex : TEXCOORD1;
                fixed4 color : COLOR;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uvOutDistTex = TRANSFORM_TEX(v.uv, _DistortTex);//得到_DistortTex空间下的uv坐标
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);//先对原本图片的纹理进行采样得到基本的颜色

                i.uvOutDistTex.x += (_Time * _DistortTexXSpeed) % 1;//将噪声纹理图和时间成比例进行移动
                i.uvOutDistTex.y += (_Time * _DistortTexYSpeed) % 1;
                float outDistortAmnt = (tex2D(_DistortTex, i.uvOutDistTex).r - 0.5) * 0.2 * _DistortAmount;//通过采样噪声图的r值来得到变形的大小参数
                float2 destUv = (0, 0);
                destUv.x += outDistortAmnt;//描边空间的xy加上这个变形的参数，使描边变形
                destUv.y += outDistortAmnt;
                float4 noiseCol = tex2D(_DistortTex, destUv);


                fixed4 emission = tex2D(_GlowTex, i.uv);//再对发光纹理图采样得到发光的颜色

                emission.rgb *= emission.a * col.a * _GlowIntensity * _GlowColor;//再乘以发光的强度和发光的颜色得到一个我们可以通过数据控制的颜色
                col.rgb += emission.rgb * noiseCol;//再让原本颜色加上发光颜色再加上扭曲颜色

                return col;
            }
            ENDCG
        }
    }
}