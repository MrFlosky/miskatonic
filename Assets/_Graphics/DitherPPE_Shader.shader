Shader "Unlit/DitherPPE_Shader"
{               
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DitherStrength ("Dither Strength", Range(0.0, 1.0)) = 0.01
        _ColorLevels ("Color Levels", Range(2, 256)) = 32
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            // texture to read from itself
            sampler2D _MainTex;
            float4 _MainTex_ST; // dont need scale and offset to transform tex coords
            // Strength of dither, higher for stronger. 0.01 default
            float _DitherStrength;
            // Number of color levels per color chnnael for quantization
            int _ColorLevels;

            // pre-computed bayer matrix since we already know their size for formula
            static const float bayerMatrix8x8[64] =
             {
                    0.0/64.0, 48.0/64.0, 12.0/64.0, 60.0/64.0,  3.0/64.0, 51.0/64.0, 15.0/64.0, 63.0/64.0,
                32.0/64.0, 16.0/64.0, 44.0/64.0, 28.0/64.0, 35.0/64.0, 19.0/64.0, 47.0/64.0, 31.0/64.0,
                    8.0/64.0, 56.0/64.0,  4.0/64.0, 52.0/64.0, 11.0/64.0, 59.0/64.0,  7.0/64.0, 55.0/64.0,
                40.0/64.0, 24.0/64.0, 36.0/64.0, 20.0/64.0, 43.0/64.0, 27.0/64.0, 39.0/64.0, 23.0/64.0,
                    2.0/64.0, 50.0/64.0, 14.0/64.0, 62.0/64.0,  1.0/64.0, 49.0/64.0, 13.0/64.0, 61.0/64.0,
                34.0/64.0, 18.0/64.0, 46.0/64.0, 30.0/64.0, 33.0/64.0, 17.0/64.0, 45.0/64.0, 29.0/64.0,
                10.0/64.0, 58.0/64.0,  6.0/64.0, 54.0/64.0,  9.0/64.0, 57.0/64.0,  5.0/64.0, 53.0/64.0,
                42.0/64.0, 26.0/64.0, 38.0/64.0, 22.0/64.0, 41.0/64.0, 25.0/64.0, 37.0/64.0, 21.0/64.0
            };

            static const float4x4 bayerMatrix4x4 = float4x4
            (
                0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                12.0/16.0, 4.0/16.0,  14.0/16.0, 6.0/16.0,
                3.0/16.0,  11.0/16.0, 1.0/16.0,  9.0/16.0,
                15.0/16.0, 7.0/16.0,  13.0/16.0, 5.0/16.0
            );

            static const float2x2 bayerMatrix2x2 = float2x2(
            0.0/4.0, 2.0/4.0,
            3.0/4.0, 1.0/4.0
            );

            float3 dither(float3 inputColor, float4 screenPos)
             {  
                // Accept RGB color, not just luminance

                // get pixel coords. convert from clip space coords to normalized 0-1 coords via manual perspective division.
                // THEN scale by screen res for pixel coords!
                float2 screenPixels = screenPos.xy / screenPos.w * _ScreenParams.xy;

                int x = int(screenPixels.x) % 8; // 8 for 8x8 bayer
                int y = int(screenPixels.y) % 8;

                // note if using different size bayer matrix, change rightside modulo to match size
                //float threshold = bayerMatrix2x2[y * 2 + x]; 
                //float threshold = bayerMatrix4x4[y][x];  // Less obvious pattern
                float threshold = bayerMatrix8x8[y * 8 + x];
                
                
                inputColor.rgb += threshold * _DitherStrength;

                // subtract 1 for 0-color level range
                float levels = float(_ColorLevels - 1);
                inputColor.r = floor(inputColor.r * levels + 0.5) / levels;
                inputColor.g = floor(inputColor.g * levels + 0.5) / levels;
                inputColor.b = floor(inputColor.b * levels + 0.5) / levels;
                
                return inputColor;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // send texture coordinate to frag shader, interpolate on the way
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex); // use screen pos for any res to index bayer matrix
                return o;
            }

            fixed4 frag (v2f terped) : SV_Target
            {
                // Sample input texture
                float3 inputColor = tex2D(_MainTex, terped.uv).rgb;
                
                // dither
                float3 finalColor = dither(inputColor, terped.screenPos);  // Pass the actual RGB color
                
                return fixed4(finalColor, 1.0);
            }

            ENDCG
        }
    }
}