Shader "Unlit/PaintPPE_Shader"
{               
    // need to put each var in cgprogram black
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius for Paint Intensity", Float) = 3.0
        //_ColorLevels ("Color Levels", Range(2, 1000)) = 8
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // How many sectors is the circular region divided up into?
            // 8 good for in between noise/texture reduction and edge preservation
            #define SECTOR_COUNT 8
            float _Radius;
            // Hardcoded texture pixel size for 1920x1080
            static const float2 TEXEL_SIZE = float2(1.0/1920.0, 1.0/1080.0);
            // texture to read from itself
            sampler2D _MainTex;
            // for color levels
            //int _ColorLevels;
            float4 _MainTex_ST; // dont need scale and offset to transform tex coords


            static const float4x4 bayerMatrix4x4 = float4x4
            (
                0.0,  8.0,  2.0, 10.0,
                12.0, 4.0,  14.0, 6.0,
                3.0,  11.0, 1.0, 9.0,
                15.0, 7.0,  13.0, 5.0
            ) / 16.0;

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

            // weighting to make it so go central pixels within sector have more impact
            // on variance and average calcs. variance lowers as closer to central region
            float polynomialWeight(float x, float y, float eta, float lambda)
            {
                float polyValue = (x + eta) - lambda * (y * y);
                return max(0.0, polyValue * polyValue);
            }

            // Sample texture at given texture coordinate with offsetwhy 
            float3 sampleTextureColor(float2 offset, float2 baseCoord)
            {
                float2 offsetTexCoord = baseCoord + offset * TEXEL_SIZE;
                return tex2D(_MainTex, offsetTexCoord).rgb;
            }


            // Get the VARIANCE (standard deviation is sqrt(variance) which we arent doing bc compute cost) and average color from a specific sector
            // within the greater region around each pixel.
            void getSectorVarianceAndAverageColor(float2 texCoord, float angle, float radius, out float3 avgColor, out float variance) 
            {
                float3 weightedColorSum = float3(0.0, 0.0, 0.0);
                float3 weightedSquaredColorSum = float3(0.0, 0.0, 0.0);
                float totalWeight = 0.0;

                // for polynomial weighting
                float eta = 0.1;
                float lambda = 0.5;

                for (float r = 1.0; r <= radius; r += 1.0) // samples at distance up to r from center in int steps
                {
                    for (float a = -0.392699; a <= 0.392699; a += 0.196349) // samples at angles covering a 45 degree range
                     {
                        // polar (Distance, angle) to cartesian conversion (x,y)
                        float2 sampleOffset = r * float2(cos(angle + a), sin(angle + a));
                        float3 color = sampleTextureColor(sampleOffset, texCoord);

                        float weight = polynomialWeight(sampleOffset.x, sampleOffset.y, eta, lambda);
                        weightedColorSum += color * weight;
                        weightedSquaredColorSum += color * color * weight;
                        totalWeight += weight;
                    }
                }

                avgColor = weightedColorSum / totalWeight;
                // calc variance per channel
                float3 sdPerChannel = (weightedSquaredColorSum / totalWeight) - (avgColor * avgColor);
                // convert to single luminance val for comparison
                variance = dot(sdPerChannel, float3(0.299, 0.587, 0.114));
            }




        float3 dither(float2 uv, float3 inputColor)
         {  
            int x = int(uv.x * 1920) % 8;
            int y = int(uv.y * 1080) % 8;
            //float threshold = bayerMatrix4x4[y][x];  // Less obvious pattern
            float threshold = bayerMatrix8x8[y * 8 + x];
            // 0.006 not bad. higher for more prominent dithering
            inputColor.rgb += threshold * 0.01;
            
            // 15-bit quantization (5 bits per channel: 32 levels each)
            float levels = 31.0;
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
                return o;
            }

            fixed4 frag (v2f terped) : SV_Target
            {

                float3 sectorAvgColors[SECTOR_COUNT];
                float sectorVariances[SECTOR_COUNT];
                
                // samples 8 wedges around each pixel, each 45 degrees wide
                for (int j = 0; j < SECTOR_COUNT; j++)
                {
                    float angle = float(j) * 6.28318 / float(SECTOR_COUNT);
                    getSectorVarianceAndAverageColor(terped.uv, angle, _Radius, sectorAvgColors[j], sectorVariances[j]);
                }

                float minVariance = sectorVariances[0];
                float3 finalColor = sectorAvgColors[0];

                // determine which sector has the smallest sd, and choose its average color
                for(int i = 1; i < SECTOR_COUNT; i++)
                {
                    if(sectorVariances[i] < minVariance)
                    {
                        minVariance = sectorVariances[i];
                        finalColor = sectorAvgColors[i];
                    }
                }

                // dither. this is being done as second render pass, only keeping incase performance needs
                //finalColor.rgb = dither(terped.uv, finalColor);  // Pass the actual RGB color
                
                return fixed4(finalColor, 1.0);
            }

            ENDCG
        }
    }
}
