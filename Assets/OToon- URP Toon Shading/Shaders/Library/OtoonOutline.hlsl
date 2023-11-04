#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
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
    half _HalftoneTilling;
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

struct VertexInput
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 tangent : TANGENT;
    float2 texcoord0 : TEXCOORD0;
};
struct VertexOutput
{
    float4 pos : SV_POSITION;
    float2 uv0 : TEXCOORD0;
    float4 screenPos : TEXCOORD1;
};

float Remap(float In, float2 InMinMax, float2 OutMinMax)
{
    return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

VertexOutput vert(VertexInput v)
{
    VertexOutput o = (VertexOutput)0;
    o.uv0 = v.texcoord0;
    float4 objPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
    VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
    float3 normalHCS = mul((float3x3)UNITY_MATRIX_VP, mul((float3x3)UNITY_MATRIX_M, v.normal));
    o.pos = vertexInput.positionCS;
    float outlineWidth = lerp(0, _OutlineWidth, Remap(max(_OutlineDistancFade.y - distance(_WorldSpaceCameraPos, vertexInput.positionWS), 0), float2(0, _OutlineDistancFade.y), float2(0, 1)));
    outlineWidth = lerp(0, outlineWidth, saturate((distance(vertexInput.positionWS, _WorldSpaceCameraPos) - _OutlineDistancFade.x) / _OutlineDistancFade.y));
    o.pos.xy += normalize(normalHCS.xy) / _ScreenParams.xy * o.pos.w * outlineWidth;

    o.screenPos = ComputeScreenPos(o.pos);
    return o;
}

//from shader graph
void UnityDither(float In, float4 ScreenPosition)
{
    float2 uv = ScreenPosition.xy * _ScreenParams.xy / _DitherTexelSize;
    float DITHER_THRESHOLDS[16] = {
        1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    clip(In - DITHER_THRESHOLDS[index]);
}

float4 frag(VertexOutput i) : SV_Target
{
    #ifndef _OUTLINE
        clip(-1);
    #endif

    UnityDither(_DitherThreshold, i.screenPos / i.screenPos.w);
    clip(-_OutlineMode);
    return float4(_OutlineColor);
}
