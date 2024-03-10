Shader "Hidden/Outline" {
    Properties {
        [Space]
        _MainTex("Texture", 2D) = "white" { }
        _OutlineColor ("OutlineColor", Color) = (1, 1, 1, 1) //描边颜色
        _OutlineAlpha ("OutlineAlpha", Range(0, 1)) = 1 //描边透明度
        _OutlinePixelWidth ("OutlinePixelWidth", Int) = 1 //描边像素点

        _OutlineDistortTex ("OutlineDistortionTex", 2D) = "white" { }//描边的变形的噪声图
        _OutlineDistortAmount ("OutlineDistortionAmount", Range(0, 2)) = 0.5 //噪声图波动的大小系数
        _OutlineDistortTexXSpeed ("OutlineDistortTexXSpeed", Range(-50, 50)) = 5 //噪声图波动的X轴速度
        _OutlineDistortTexYSpeed ("OutlineDistortTexYSpeed", Range(-50, 50)) = 5 //噪声图波动的Y轴速度
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
            float4 _MainTex_TexelSize;//_MainTex纹理的每个像素的尺寸

            fixed4 _OutlineColor;
            float _OutlineAlpha;
            int _OutlinePixelWidth;

            sampler2D _OutlineDistortTex;
            float4 _OutlineDistortTex_ST;
            float _OutlineDistortTexXSpeed, _OutlineDistortTexYSpeed, _OutlineDistortAmount;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float2 uvOutDistTex : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvOutDistTex = TRANSFORM_TEX(v.uv, _OutlineDistortTex);//得到_OutlineDistortTex空间下的uv坐标
                o.uv = v.uv;
                return o;
            }



            fixed4 frag(v2f i) : SV_Target {
                //------Outline------
                fixed4 col = tex2D(_MainTex, i.uv);//对主纹理进行采样
                float originalAlpha = col.a;//原始的alpha值

                float2 destUv = float2(_OutlinePixelWidth * _MainTex_TexelSize.x, _OutlinePixelWidth * _MainTex_TexelSize.y);//得到描边空间的像素大小

                i.uvOutDistTex.x += (_Time * _OutlineDistortTexXSpeed) % 1;//将噪声纹理图和时间成比例进行移动
                i.uvOutDistTex.y += (_Time * _OutlineDistortTexYSpeed) % 1;

                //通过采样噪声图的r值来得到变形的大小参数
                float outDistortAmnt = (tex2D(_OutlineDistortTex, i.uvOutDistTex).r - 0.5) * 0.2 * _OutlineDistortAmount;
                destUv.x += outDistortAmnt;//描边空间的xy加上这个变形的参数，使描边变形
                destUv.y += outDistortAmnt;

                //得到八个方向的外描边的边界值的alpha值 因为是外描边所以要加上destUv组成的float2值
                float spriteLeft = tex2D(_MainTex, i.uv + float2(destUv.x, 0)).a;
                float spriteRight = tex2D(_MainTex, i.uv - float2(destUv.x, 0)).a;
                float spriteBottom = tex2D(_MainTex, i.uv + float2(0, destUv.y)).a;
                float spriteTop = tex2D(_MainTex, i.uv - float2(0, destUv.y)).a;
                float spriteTopLeft = tex2D(_MainTex, i.uv + float2(destUv.x, destUv.y)).a;
                float spriteTopRight = tex2D(_MainTex, i.uv + float2(-destUv.x, destUv.y)).a;
                float spriteBotLeft = tex2D(_MainTex, i.uv + float2(destUv.x, -destUv.y)).a;
                float spriteBotRight = tex2D(_MainTex, i.uv + float2(-destUv.x, -destUv.y)).a;
                float result = spriteLeft + spriteRight + spriteBottom + spriteTop + spriteTopLeft + spriteTopRight + spriteBotLeft + spriteBotRight;

                result = step(0.05, saturate(result));//如果最后的结果alpha值大于0.05，则为1，否则就是0（也就是边界判定）
                result *= (1 - originalAlpha) * _OutlineAlpha;//控制描边的alpha值

                fixed4 outline = _OutlineColor;//描边的颜色
                col = lerp(col, outline, result);//插值采样得到最后的颜色

                //------Rotate------


                return col;
            }
            ENDCG
        }
    }
}