Shader "Vefects/SH_Vefects_VFX_SRP_Heat_Haze_01"
{
    Properties
    {
        _Texture("Texture", 2D) = "white" {}
        _TextureChannel("Texture Channel", Vector) = (0,1,0,0)
        _DistortionStrength("Distortion Strength", Float) = 7
        _DissolveMask("Dissolve Mask", 2D) = "white" {}
        [Space(13)][Header(AR)][Space(13)]_Cull1("Cull", Float) = 2
        _Src1("Src", Float) = 5
        _Dst1("Dst", Float) = 10
        _ZWrite1("ZWrite", Float) = 0
        _ZTest1("ZTest", Float) = 2
        _DissolveUVS("Dissolve UV S", Vector) = (0,0,0,0)
        _DissolveUVP("Dissolve UV P", Vector) = (0,0,0,0)
        [HideInInspector] _texcoord("", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+0" "IsEmissive"="true" "RenderPipeline"="UniversalPipeline" }
        Cull [_Cull1]
        ZWrite [_ZWrite1]
        ZTest [_ZTest1]
        Blend [_Src1] [_Dst1]

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex   : POSITION;
                float4 uv       : TEXCOORD0;    // xy = uv, z = dissolve offset from custom data
                float4 color    : COLOR;
            };

            struct Varyings
            {
                float4 pos          : SV_POSITION;
                float4 uv           : TEXCOORD0;
                float4 color        : COLOR;
                float4 screenPos    : TEXCOORD1;
            };

            // Textures
            TEXTURE2D(_Texture);        SAMPLER(sampler_Texture);
            TEXTURE2D(_DissolveMask);   SAMPLER(sampler_DissolveMask);

            // _CameraOpaqueTexture is URP's equivalent of GrabPass _GrabTexture
            TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _Texture_ST;
                float4 _TextureChannel;
                float2 _DissolveUVS;
                float2 _DissolveUVP;
                float _DistortionStrength;
                float _Cull1;
                float _Src1;
                float _Dst1;
                float _ZWrite1;
                float _ZTest1;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.pos       = TransformObjectToHClip(v.vertex.xyz);
                o.uv        = v.uv;
                o.color     = v.color;
                // compute screen position for sampling the opaque texture
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Normalized screen UV for sampling the opaque (grab) texture
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                // Sample distortion texture, dot with channel mask to pick a single channel
                float2 uv_Texture = i.uv.xy * _Texture_ST.xy + _Texture_ST.zw;
                float dotResult = dot(SAMPLE_TEXTURE2D(_Texture, sampler_Texture, uv_Texture), _TextureChannel);

                // Dissolve mask with panning UVs
                float2 panner = _Time.y * _DissolveUVP + (i.uv.xy * _DissolveUVS);
                float dissolveMask = saturate(SAMPLE_TEXTURE2D(_DissolveMask, sampler_DissolveMask, panner).r);

                // Combined distortion amount — vertex alpha * (distortion * (dissolveMask + uv.z custom data))
                float distortionAmount = i.color.a * (saturate(dotResult) * (dissolveMask + i.uv.z));

                // Offset the screen UV by distortion amount to create heat haze
                // divide by _ScreenParams.xy to convert pixel-space distortion to UV-space
                float2 distortedUV = screenUV + (distortionAmount * _DistortionStrength * (1.0 / _ScreenParams.xy));

                // Sample the scene behind this particle using the distorted screen UV
                half3 sceneColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, distortedUV).rgb;

                float alpha = saturate(distortionAmount);

                return half4(sceneColor, alpha);
            }
            ENDHLSL
        }
    }
}