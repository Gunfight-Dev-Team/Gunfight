Shader "Hidden/Outline" {
    Properties {
        [Space]
        _MainTex("Texture", 2D) = "white" { }
        _OutlineColor ("OutlineColor", Color) = (1, 1, 1, 1)
        _OutlineAlpha ("OutlineAlpha", Range(0, 1)) = 1
        _OutlinePixelWidth ("OutlinePixelWidth", Int) = 1

        _OutlineDistortTex ("OutlineDistortionTex", 2D) = "white" { }
        _OutlineDistortAmount ("OutlineDistortionAmount", Range(0, 2)) = 0.5
        _OutlineDistortTexXSpeed ("OutlineDistortTexXSpeed", Range(-50, 50)) = 5
        _OutlineDistortTexYSpeed ("OutlineDistortTexYSpeed", Range(-50, 50)) = 5
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
            float4 _MainTex_TexelSize;

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
                o.uvOutDistTex = TRANSFORM_TEX(v.uv, _OutlineDistortTex);
                o.uv = v.uv;
                return o;
            }



            fixed4 frag(v2f i) : SV_Target {
                //------Outline------
                fixed4 col = tex2D(_MainTex, i.uv);
                float originalAlpha = col.a;

                float2 destUv = float2(_OutlinePixelWidth * _MainTex_TexelSize.x, _OutlinePixelWidth * _MainTex_TexelSize.y);

                i.uvOutDistTex.x += (_Time * _OutlineDistortTexXSpeed) % 1;
                i.uvOutDistTex.y += (_Time * _OutlineDistortTexYSpeed) % 1;

                float outDistortAmnt = (tex2D(_OutlineDistortTex, i.uvOutDistTex).r - 0.5) * 0.2 * _OutlineDistortAmount;
                destUv.x += outDistortAmnt;
                destUv.y += outDistortAmnt;

                float spriteLeft = tex2D(_MainTex, i.uv + float2(destUv.x, 0)).a;
                float spriteRight = tex2D(_MainTex, i.uv - float2(destUv.x, 0)).a;
                float spriteBottom = tex2D(_MainTex, i.uv + float2(0, destUv.y)).a;
                float spriteTop = tex2D(_MainTex, i.uv - float2(0, destUv.y)).a;
                float spriteTopLeft = tex2D(_MainTex, i.uv + float2(destUv.x, destUv.y)).a;
                float spriteTopRight = tex2D(_MainTex, i.uv + float2(-destUv.x, destUv.y)).a;
                float spriteBotLeft = tex2D(_MainTex, i.uv + float2(destUv.x, -destUv.y)).a;
                float spriteBotRight = tex2D(_MainTex, i.uv + float2(-destUv.x, -destUv.y)).a;
                float result = spriteLeft + spriteRight + spriteBottom + spriteTop + spriteTopLeft + spriteTopRight + spriteBotLeft + spriteBotRight;

                result = step(0.05, saturate(result));
                result *= (1 - originalAlpha) * _OutlineAlpha;

                fixed4 outline = _OutlineColor;
                col = lerp(col, outline, result);

                //------Rotate------


                return col;
            }
            ENDCG
        }
    }
}