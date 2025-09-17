Shader "Unlit/PaintPPE_Shader"
{               
    // need to put each var in cgprogram black
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius", Float) = 3.0
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
            #define SECTOR_COUNT 6
            float _Radius;
            // Hardcoded texture pixel size for 1920x1080
            static const float2 TEXEL_SIZE = float2(1.0/1920.0, 1.0/1080.0);
            // texture to read from itself
            sampler2D _MainTex;
            float4 _MainTex_ST; // dont need scale and offset to transform tex coords

            
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


            // Get the standard deviation and average color from a specific sector
            // within the greater region around each pixel
            void getSectorVarianceAndAverageColor(float2 texCoord, float angle, float radius, out float3 avgColor, out float variance) 
            {
                float3 weightedColorSum = float3(0.0, 0.0, 0.0);
                float3 weightedSquaredColorSum = float3(0.0, 0.0, 0.0);
                float totalWeight = 0.0;

                // for polynomial weighting
                float eta = 0.1;
                float lambda = 0.5;

                for (float r = 1.0; r <= radius; r += 1.0)
                {
                    for (float a = -0.392699; a <= 0.392699; a += 0.196349) // creates wedge shaped sector
                     {
                        float2 sampleOffset = r * float2(cos(angle + a), sin(angle + a));
                        float3 color = sampleTextureColor(sampleOffset, texCoord);

                        float weight = polynomialWeight(sampleOffset.x, sampleOffset.y, eta, lambda);
                        weightedColorSum += color * weight;
                        weightedSquaredColorSum += color * color * weight;
                        totalWeight += weight;
                    }
                }

                avgColor = weightedColorSum / totalWeight; // cast sample count to int before div
                // calc standard deviation (actually variance? why) per channel
                float3 sdPerChannel = (weightedSquaredColorSum / totalWeight) - (avgColor * avgColor);
                // convert to single luminance val for comparison
                variance = dot(sdPerChannel, float3(0.299, 0.587, 0.114));
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

                for (int j = 0; j < SECTOR_COUNT; j++) {
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

                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}
