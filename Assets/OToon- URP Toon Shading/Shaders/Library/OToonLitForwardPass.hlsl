#ifndef OTOON_LIT_PASS_INCLUDED
#define OTOON_LIT_PASS_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Lighting_Extend.hlsl"
#include "OToonPBRSurfaceData.hlsl"

// keep this file in sync with LitGBufferPass.hlsl

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 texcoord : TEXCOORD0;
    float2 lightmapUV : TEXCOORD1;
    float2 faceShadowUV : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
    float3 positionWS : TEXCOORD2;

    float3 normalWS : TEXCOORD3;
    #if defined(_NORMALMAP)
        float4 tangentWS : TEXCOORD4;    // xyz: tangent, w: sign
    #endif
    float3 viewDirWS : TEXCOORD5;

    half4 fogFactorAndVertexLight : TEXCOORD6; // x: fogFactor, yzw: vertex light

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        float4 shadowCoord : TEXCOORD7;
    #endif

    float4 screenPos : TEXCOORD8;
    float3 originWS : TEXCOORD9;
    float4 spos : TEXCOORD10;
    float3 bitangent : TEXCOORD11;
    float3 frontDirectionWS : TEXCOORD12;
    float3 rightDirectionWS : TEXCOORD13;
    float2 faceShadowUV : TEXCOORD14;

    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        inputData.positionWS = input.positionWS;
    #endif

    #if VERSION_GREATER_EQUAL(10, 0)
        inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
        inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
    #endif

    half3 viewDirWS = SafeNormalize(input.viewDirWS);
    #if defined(_NORMALMAP) || defined(_DETAIL)
        float sgn = input.tangentWS.w;      // should be either +1 or -1
        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
        inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
    #else
        inputData.normalWS = input.normalWS;
    #endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
        inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
    #else
        inputData.shadowCoord = float4(0, 0, 0, 0);
    #endif

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 viewDirWS = normalize(GetCameraPositionWS() - vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
    output.bitangent = normalInput.bitangentWS;

    output.viewDirWS = viewDirWS;
    #if defined _NORMALMAP
        real sign = input.tangentOS.w * GetOddNegativeScale();
        output.tangentWS = half4(normalInput.tangentWS.xyz, sign);
    #endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
    output.positionWS = vertexInput.positionWS;

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

    output.positionCS = vertexInput.positionCS;
    output.screenPos = ComputeScreenPos(output.positionCS);
    output.spos = output.positionCS;
    output.originWS = TransformObjectToWorld(float3(0, 0, 0));
    output.frontDirectionWS = TransformObjectToWorldDir(_FaceFrontDirection);
    output.rightDirectionWS = TransformObjectToWorldDir(_FaceRightDirection);
    output.faceShadowUV = input.faceShadowUV;
    return output;
}

float2 GetScreenUV(float2 clipPos, float UVscaleFactor)
{
    float4 SSobjectPosition = mul(UNITY_MATRIX_MVP, float4(0, 0, 0, 1.0)) ;
    float2 screenUV = clipPos.xy;
    float screenRatio = _ScreenParams.y / _ScreenParams.x;

    screenUV.x -= SSobjectPosition.x / (SSobjectPosition.w);
    screenUV.y -= SSobjectPosition.y / (SSobjectPosition.w);

    screenUV.y *= screenRatio;
    screenUV *= SSobjectPosition.w;

    return screenUV;
};

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

// Used in Standard (Physically Based) shader
half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    UnityDither(_DitherThreshold, input.screenPos / input.screenPos.w);

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

    input.normalWS = lerp(input.normalWS, normalize(float3(input.positionWS - _SpherizeNormalOrigin.xyz)), _SpherizeNormalEnabled);
    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);

    OtoonPBRSurfaceData otoonSurfaceData;
    InitializeOtoonPBRSurfaceData(otoonSurfaceData);
    otoonSurfaceData.posWS = input.positionWS;
    otoonSurfaceData.originPosWS = input.originWS;
    otoonSurfaceData.bitangent = input.bitangent;
    otoonSurfaceData.frontDirectionWS = input.frontDirectionWS;
    otoonSurfaceData.rightDirectionWS = input.rightDirectionWS;
    otoonSurfaceData.faceShadowMapUV = input.faceShadowUV;

    half4 spos = TransformWorldToHClip(input.positionWS);
    half2 clipUv = input.spos.xy / input.spos.w;
    half4 cpos = TransformObjectToHClip(float3(0, 0, 0));
    clipUv -= cpos.xy / cpos.w;
    clipUv *= cpos.w / UNITY_MATRIX_P._m11;
    clipUv.x *= _ScreenParams.x / _ScreenParams.y;

    half4 pbrColor = UniversalFragmentPBR_Extend(inputData, surfaceData, input.uv, clipUv, otoonSurfaceData);
    half4 color = pbrColor;

    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, _Surface);
    return color;
}

#endif
