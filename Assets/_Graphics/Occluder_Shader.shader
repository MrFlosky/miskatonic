Shader "Custom/Occluder"
{
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }
        
        Pass
        {
            // Write to depth buffer but not color buffer
            ColorMask 0  // Don't write any color channels
            ZWrite On    // Write to depth buffer
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz); // URP equivalent of UnityObjectToClipPos
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return half4(0, 0, 0, 0); // Doesn't matter, ColorMask 0 discards it
            }
            ENDHLSL
        }
    }
}