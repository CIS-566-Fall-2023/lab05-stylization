#ifndef OTOON_CUSTOM_LIT_INPUT_INCLUDED
#define OTOON_CUSTOM_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "OtoonPBRSurfaceData.hlsl"
CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half4 _SpecColor;
    half4 _EmissionColor;
    half _Cutoff;
    half _Smoothness;
    half _Metallic;
    half _BumpScale;
    half _OcclusionStrength;
    half _Surface;
    half _DitherTexelSize;
    half _DitherThreshold;
    half _StepViaRampTexture;
    half _NoiseScale;
    half _NoiseStrength;
    half _ToonBlending;
    half _DiffuseStep;
    half _HalfToneUvMode;
    half _SpecClipMaskScale;
    half _SpecularClipStrength;
    half _SpecularFalloff;
    half _SpecularSize;
    half _RimPower;
    half _RimLightAlign;
    half _RimLightSmoothness;
    half4 _RimColor;
    half _HalfToneEnabled;
    half4 _HalfToneColor;
    half _HalftoneNoiseClip;
    half _BrushLowerCut;
    half _BrushSize;
    float _HalftoneTilling;
    half _HalfToneDiffuseStep;
    half _SizeFalloff;
    half _HalfToneIncludeReceivedShadow;
    half _HalftoneFadeDistance;
    half _HalftoneFadeToColor;
    half4 _OutlineColor;
    half _OutlineWidth;
    half2 _OutlineDistancFade;
    half _OutlineMode;
    half4 _ShadowColor;
    half _SpecShadowStrength;
    half4 _HairSpecColor;
    half _EnabledHairSpec;
    float4 _SpherizeNormalOrigin;
    half _SpherizeNormalEnabled;
    half _HatchingEnabled;
    half _HatchingDensity;
    half _HatchingRotation;
    half _HatchingDrawStrength;
    half _HatchingSmoothness;
    half _HatchingUpperBound;
    half _HatchingDiffuseOffset;
    half4 _HatchingColor;
    half _UseRampColor;
    half _FlattenGI;

    float4 _HalfTonePatternMap_ST;
    float4 _HalfToneNoiseMap_ST;
    float4 _HatchingNoiseMap_ST;
    float4 _HairSpecNoiseMap_ST;
    float _OverrideShadowColor;
    float _HairSpecNoiseStrength;
    float _HairSpecExponent;
    float _HairSpecScale;
    float _HairSpecularSize;
    float _HairSpecularSmoothness;
    float _FaceShadowMapEnabled;
    float _FaceShadowMapPow;
    float _FaceShadowSmoothness;
    half3 _FaceFrontDirection;
    half3 _FaceRightDirection;
CBUFFER_END

TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);

half4 SampleSpecularSmoothness(half2 uv, half alpha, half4 specColor, TEXTURE2D_PARAM(specMap, sampler_specMap))
{
    half4 specularSmoothness = half4(0.0h, 0.0h, 0.0h, 1.0h);
    #ifdef _SPECGLOSSMAP
        specularSmoothness = SAMPLE_TEXTURE2D(specMap, sampler_specMap, uv) * specColor;
    #elif defined(_SPECULAR_COLOR)
        specularSmoothness = specColor;
    #endif

    #ifdef _GLOSSINESS_FROM_BASE_ALPHA
        specularSmoothness.a = exp2(10 * alpha + 1);
    #else
        specularSmoothness.a = exp2(10 * specularSmoothness.a + 1);
    #endif

    return specularSmoothness;
}

inline void InitializeSimpleLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    outSurfaceData = (SurfaceData)0;

    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = albedoAlpha.a * _BaseColor.a;
    AlphaDiscard(outSurfaceData.alpha, _Cutoff);

    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    #ifdef _ALPHAPREMULTIPLY_ON
        outSurfaceData.albedo *= outSurfaceData.alpha;
    #endif

    //half4 specularSmoothness = SampleSpecularSmoothness(uv, outSurfaceData.alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
    outSurfaceData.metallic = 0.0; // unused
    outSurfaceData.specular = 0.0; // unused
    outSurfaceData.smoothness = 0.0; // unused
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    outSurfaceData.occlusion = 1.0; // unused
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}

void InitializeOtoonSimpleSurfaceData(out OtoonPBRSurfaceData otoonSurfaceData)
{
    otoonSurfaceData.stepViaRampTexture = _StepViaRampTexture;
    otoonSurfaceData.noiseScale = _NoiseScale;
    otoonSurfaceData.noiseStrength = _NoiseStrength;
    otoonSurfaceData.toonBlending = 0;
    otoonSurfaceData.diffuseStep = _DiffuseStep;
    otoonSurfaceData.halfToneUvMode = _HalfToneUvMode;
    otoonSurfaceData.specularFalloff = _SpecularFalloff;
    otoonSurfaceData.specularSize = _SpecularSize;
    otoonSurfaceData.rimPower = _RimPower;
    otoonSurfaceData.rimLightAlign = _RimLightAlign;
    otoonSurfaceData.rimLightSmoothness = _RimLightSmoothness;
    otoonSurfaceData.rimColor = _RimColor;
    otoonSurfaceData.halfToneEnabled = _HalfToneEnabled;
    otoonSurfaceData.halfToneColor = _HalfToneColor;
    otoonSurfaceData.halftoneNoiseClip = _HalftoneNoiseClip;
    otoonSurfaceData.brushLowerCut = _BrushLowerCut;
    otoonSurfaceData.brushSize = _BrushSize;
    otoonSurfaceData.brushTilling = _HalftoneTilling;
    otoonSurfaceData.halfToneDiffuseStep = _HalfToneDiffuseStep;
    otoonSurfaceData.sizeFalloff = _SizeFalloff;
    otoonSurfaceData.halfToneIncludeReceivedShadow = _HalfToneIncludeReceivedShadow;
    otoonSurfaceData.outlineColor = _OutlineColor;
    otoonSurfaceData.outlineWidth = _OutlineWidth;
    otoonSurfaceData.shadowColor = _ShadowColor;
    otoonSurfaceData.specShadowStrength = _SpecShadowStrength;
    otoonSurfaceData.specClipStrength = _SpecularClipStrength;
    otoonSurfaceData.specClipMaskScale = _SpecClipMaskScale;
    otoonSurfaceData.hairSpecColor = _HairSpecColor;
    otoonSurfaceData.originPosWS = 0;
    otoonSurfaceData.posWS = 0;
    otoonSurfaceData.bitangent = 0;
    otoonSurfaceData.hatchingEnabled = _HatchingEnabled;
    otoonSurfaceData.hatchingDensity = _HatchingDensity;
    otoonSurfaceData.hatchingRotation = _HatchingRotation;
    otoonSurfaceData.hatchingDrawStrength = _HatchingDrawStrength;
    otoonSurfaceData.hatchingSmoothness = _HatchingSmoothness;
    otoonSurfaceData.hatchingDiffuseOffset = _HatchingDiffuseOffset;
    otoonSurfaceData.hatchingColor = _HatchingColor;
    otoonSurfaceData.frontDirectionWS = 0;
    otoonSurfaceData.rightDirectionWS = 0;
    otoonSurfaceData.faceShadowMapUV = 0;
    otoonSurfaceData.halftoneFadeDistance = _HalftoneFadeDistance;
    otoonSurfaceData.halftoneFadeToColor = _HalftoneFadeToColor;
}

#endif
