Shader "Vefects/SH_Vefects_VFX_SRP_Particles_Erosion_01"
{
    Properties
    {
        _Noise_01_Texture("Noise_01_Texture", 2D) = "white" {}
        _Noise_02_Texture("Noise_02_Texture", 2D) = "white" {}
        _MaskTexture("Mask Texture", 2D) = "white" {}
        _MaskMoveTexture("Mask Move Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,0)
        _NoiseDistortionTexture("Noise Distortion Texture", 2D) = "white" {}
        _Noise01Scale("Noise 01 Scale", Vector) = (0.8,0.8,0,0)
        _Noise02Scale("Noise 02 Scale", Vector) = (1,1,0,0)
        _NoiseDistortionScale("Noise Distortion Scale", Vector) = (1,1,0,0)
        _Noise01Speed("Noise 01 Speed", Vector) = (0.5,0.5,0,0)
        _Noise02Speed("Noise 02 Speed", Vector) = (-0.2,0.4,0,0)
        _MaskMoveScale("Mask Move Scale", Vector) = (1,1,0,0)
        _MaskScale("Mask Scale", Vector) = (1,1,0,0)
        _MaskOffset("Mask Offset", Vector) = (0,0,0,0)
        _NoiseDistortionSpeed("Noise Distortion Speed", Vector) = (0.2,0.25,0,0)
        _MaskMultiply("Mask Multiply", Float) = 1
        _MaskMoveMultiply("Mask Move Multiply", Float) = 1
        _NoisesMultiply("Noises Multiply", Float) = 1
        _MaskPower("Mask Power", Float) = 1
        _NoisesPower("Noises Power", Float) = 1
        _MaskMovePower("Mask Move Power", Float) = 1
        _Distortion("Distortion", Float) = 1
        _DistortionIntensity("Distortion Intensity", Float) = 0
        _OpacityBoost("Opacity Boost", Float) = 5
        _EmissionIntensity("Emission Intensity", Float) = 1
        _WindSpeed("Wind Speed", Float) = 1
        _MaskSpeed("Mask Speed", Float) = 0
        _Dissolve("Dissolve", Float) = 0
        [Space(13)][Header(AR)][Space(13)]_Cull1("Cull", Float) = 2
        _Src1("Src", Float) = 5
        _Dst1("Dst", Float) = 10
        _ZWrite1("ZWrite", Float) = 0
        _ZTest1("ZTest", Float) = 2
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
                float4 vertex       : POSITION;
                float2 uv           : TEXCOORD0;
                float4 uv2          : TEXCOORD1;  // uv2_texcoord2: x=dissolve data, zw=mask move offset
                float4 color        : COLOR;
            };

            struct Varyings
            {
                float4 pos          : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float4 uv2          : TEXCOORD1;
                float4 color        : COLOR;
            };

            // Textures
            TEXTURE2D(_Noise_01_Texture);       SAMPLER(sampler_Noise_01_Texture);
            TEXTURE2D(_Noise_02_Texture);       SAMPLER(sampler_Noise_02_Texture);
            TEXTURE2D(_MaskTexture);            SAMPLER(sampler_MaskTexture);
            TEXTURE2D(_MaskMoveTexture);        SAMPLER(sampler_MaskMoveTexture);
            TEXTURE2D(_NoiseDistortionTexture); SAMPLER(sampler_NoiseDistortionTexture);

            // Properties — must be inside CBUFFER for SRP Batcher compatibility
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float2 _Noise01Scale;
                float2 _Noise02Scale;
                float2 _NoiseDistortionScale;
                float2 _Noise01Speed;
                float2 _Noise02Speed;
                float2 _MaskMoveScale;
                float2 _MaskScale;
                float2 _MaskOffset;
                float2 _NoiseDistortionSpeed;
                float _MaskMultiply;
                float _MaskMoveMultiply;
                float _NoisesMultiply;
                float _MaskPower;
                float _NoisesPower;
                float _MaskMovePower;
                float _Distortion;
                float _DistortionIntensity;
                float _OpacityBoost;
                float _EmissionIntensity;
                float _WindSpeed;
                float _MaskSpeed;
                float _Dissolve;
                float _Cull1;
                float _Src1;
                float _Dst1;
                float _ZWrite1;
                float _ZTest1;
            CBUFFER_END

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.pos   = TransformObjectToHClip(v.vertex.xyz);
                o.uv    = v.uv;
                o.uv2   = v.uv2;
                o.color = v.color;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Wind speed — drives all animation
                float windSpeed = _WindSpeed * _Time.y;

                // Distortion — samples a noise texture and uses it to offset all other UVs
                float2 uv_NoiseDistortion = i.uv * _NoiseDistortionScale;
                float2 panner_Distortion  = windSpeed * _NoiseDistortionSpeed + uv_NoiseDistortion;
                float Distortion = (SAMPLE_TEXTURE2D(_NoiseDistortionTexture, sampler_NoiseDistortionTexture, panner_Distortion).r * 0.1) * _Distortion;

                // Mask — stationary shaped mask with optional speed
                float2 appendMaskSpeed    = float2(_MaskSpeed, 0.0);
                float2 uv_Mask            = i.uv * _MaskScale + _MaskOffset;
                float2 panner_Mask        = windSpeed * appendMaskSpeed + uv_Mask;
                float maskSample          = SAMPLE_TEXTURE2D(_MaskTexture, sampler_MaskTexture, (Distortion * _DistortionIntensity) + panner_Mask).r;
                float Mask                = saturate(pow(maskSample, _MaskPower) * _MaskMultiply);

                // Mask Move — secondary mask driven by particle's UV2 zw channels (custom data)
                float2 uv_MaskMove        = i.uv * _MaskMoveScale;
                float2 maskMoveOffset     = float2(i.uv2.z, i.uv2.w);
                float maskMoveSample      = SAMPLE_TEXTURE2D(_MaskMoveTexture, sampler_MaskMoveTexture, uv_MaskMove + maskMoveOffset).r;
                float MaskMove            = saturate(pow(maskMoveSample, _MaskMovePower) * _MaskMoveMultiply);

                // Noises — two noise textures multiplied together
                float2 uv_Noise01         = i.uv * _Noise01Scale;
                float2 panner_Noise01     = windSpeed * _Noise01Speed + uv_Noise01;
                float2 uv_Noise02         = i.uv * _Noise02Scale;
                float2 panner_Noise02     = windSpeed * _Noise02Speed + uv_Noise02;
                float noise01             = SAMPLE_TEXTURE2D(_Noise_01_Texture, sampler_Noise_01_Texture, Distortion + panner_Noise01).r;
                float noise02             = SAMPLE_TEXTURE2D(_Noise_02_Texture, sampler_Noise_02_Texture, Distortion + panner_Noise02).r;
                float Noises              = saturate(pow(noise01 * noise02, _NoisesPower) * _NoisesMultiply);

                // Combine mask * maskmove * noises into single combined alpha shape
                float combinedShape       = saturate(Mask * MaskMove) * Noises;

                // Emission — color tinted by particle vertex color, driven by combined shape
                half3 emission            = (((_Color.rgb * i.color.rgb) * combinedShape) * _EmissionIntensity);

                // Alpha — combined shape boosted for opacity, minus dissolve from UV2.x
                float dissolveInput       = i.uv2.x + _Dissolve;
                float alpha               = saturate(saturate(combinedShape * _OpacityBoost) - dissolveInput);
                alpha                     = saturate(i.color.a * alpha);

                return half4(emission, alpha);
            }
            ENDHLSL
        }
    }
}