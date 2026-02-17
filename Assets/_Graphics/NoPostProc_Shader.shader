Shader "Hidden/AlphaComposite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
        // ZWrite Off fullscreen blit, depth buffer operations bypassed so this is useless for transparent objs to not block other transparent objects
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Blit.hlsl provides Vert and _BlitTexture automatically.
            // _MainTex is no longer needed — URP binds the source RT to _BlitTexture instead.

            half4 frag(Varyings input) : SV_Target
            {
                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
            }
            ENDHLSL
        }
    }
}