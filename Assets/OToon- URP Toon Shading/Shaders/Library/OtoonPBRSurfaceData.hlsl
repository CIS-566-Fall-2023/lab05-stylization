#ifndef OTOON_PBR_SURFACE_DATA_INCLUDED
#define OTOON_PBR_SURFACE_DATA_INCLUDED

struct OtoonPBRSurfaceData
{
    half stepViaRampTexture;
    half noiseScale;
    half noiseStrength;
    half toonBlending;
    half diffuseStep;
    half halfToneUvMode;
    half specularFalloff;
    half specularSize;
    half rimPower;
    half rimLightAlign;
    half rimLightSmoothness;
    half4 rimColor;
    half halfToneEnabled;
    half4 halfToneColor;
    half halftoneNoiseClip;
    half brushLowerCut;
    half brushSize;
    float brushTilling;
    half halfToneDiffuseStep;
    half sizeFalloff;
    half halfToneIncludeReceivedShadow;
    half4 outlineColor;
    half outlineWidth;
    half4 shadowColor;
    half specShadowStrength;
    half specClipStrength;
    half specClipMaskScale;
    half4 hairSpecColor;
    float3 originPosWS;
    float3 posWS;
    half3 bitangent;
    half hatchingEnabled;
    half hatchingDensity;
    half hatchingRotation;
    half hatchingDrawStrength;
    half hatchingSmoothness;
    half hatchingDiffuseOffset;
    half4 hatchingColor;
    half3 frontDirectionWS;
    half3 rightDirectionWS;
    float2 faceShadowMapUV;
    half halftoneFadeDistance;
    half halftoneFadeToColor;
};

#endif