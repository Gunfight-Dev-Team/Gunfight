Shader "Hidden/Distortion" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        _DistortTex ("Distortion Texture", 2D) = "white" { }//扭曲的噪声图
        _DistortAmount ("Distortion Amount", Range(0, 2)) = 0.5 //扭曲的程度
        _DistortTexXSpeed ("Scroll speed X", Range(-50, 50)) = 5 //x轴速度
        _DistortTexYSpeed ("Scroll speed Y", Range(-50, 50)) = 5 //y轴速度

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
            sampler2D _DistortTex;
            half4 _DistortTex_ST;
            half _DistortTexXSpeed, _DistortTexYSpeed, _DistortAmount;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float2 uvDistTex : TEXCOORD3;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uvDistTex = TRANSFORM_TEX(v.uv, _DistortTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                i.uvDistTex.x += (_Time * _DistortTexXSpeed) % 1;//根据时间控制了对扭曲纹理的采样位置。通过取模运算，确保了采样位置在[0, 1]范围内
                i.uvDistTex.y += (_Time * _DistortTexYSpeed) % 1;//同理
                ////根据从噪声图中r进行扭曲强度的计算 -0.5是要将[0,1]映射到[-0.5, 0.5]
                half distortAmnt = (tex2D(_DistortTex, i.uvDistTex).r - 0.5) * 0.2 * _DistortAmount;
                i.uv.x += distortAmnt;//根据计算出的扭曲强度让i.uv进行了偏移
                i.uv.y += distortAmnt;//同理
                fixed4 col = tex2D(_MainTex, i.uv);//采样

                return col;
            }
            ENDCG
        }
    }
}