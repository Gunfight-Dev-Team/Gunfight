Shader "Hidden/ColorSwap" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }
        [NoScaleOffset] _ColorSwapTex ("Color Swap Texture", 2D) = "black" { }
        [HDR]_ColorSwapRed ("Red Channel", Color) = (1, 1, 1, 1)
        _ColorSwapRedLuminosity ("Red luminosity", Range(-1, 1)) = 0.5
        [HDR]_ColorSwapGreen ("Green Channel", Color) = (1, 1, 1, 1)
        _ColorSwapGreenLuminosity ("Green luminosity", Range(-1, 1)) = 0.5
        [HDR]_ColorSwapBlue ("Blue Channel", Color) = (1, 1, 1, 1)
        _ColorSwapBlueLuminosity ("Blue luminosity", Range(-1, 1)) = 0.5
        _ColorSwapBlend ("Color Swap Blend", Range(0, 1)) = 1

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
            sampler2D _ColorSwapTex;
            fixed4 _ColorSwapRed, _ColorSwapGreen, _ColorSwapBlue;
            float _ColorSwapRedLuminosity, _ColorSwapGreenLuminosity, _ColorSwapBlueLuminosity, _ColorSwapBlend;

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
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float luminance = 0.3 * col.r + 0.59 * col.g + 0.11 * col.b;
                half4 swapMask = tex2D(_ColorSwapTex, i.uv);
                swapMask.rgb *= swapMask.a;
                half3 redSwap = _ColorSwapRed * swapMask.r * saturate(luminance + _ColorSwapRedLuminosity);
                half3 greenSwap = _ColorSwapGreen * swapMask.g * saturate(luminance + _ColorSwapGreenLuminosity);
                half3 blueSwap = _ColorSwapBlue * swapMask.b * saturate(luminance + _ColorSwapBlueLuminosity);
                swapMask.rgb = col.rgb * saturate(1 - swapMask.r - swapMask.g - swapMask.b);
                col.rgb = lerp(col.rgb, swapMask.rgb + redSwap + greenSwap + blueSwap, _ColorSwapBlend);

                return col;
            }
            ENDCG
        }
    }
}