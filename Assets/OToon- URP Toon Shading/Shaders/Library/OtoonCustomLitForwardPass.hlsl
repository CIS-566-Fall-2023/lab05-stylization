#ifndef OTOON_CUSTOM_LIT_PASS_INCLUDED
#define OTOON_CUSTOM_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Lighting_Extend.hlsl"
#include "OToonPBRSurfaceData.hlsl"

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

    float3 posWS : TEXCOORD2;    // xyz: posWS

    #ifdef _NORMALMAP
        float4 normal : TEXCOORD3;    // xyz: normal, w: viewDir.x
        float4 tangent : TEXCOORD4;    // xyz: tangent, w: viewDir.y
        float4 bitangent : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
    #else
        float4 normal : TEXCOORD3;
        float3 viewDir : TEXCOORD4;
        float4 bitangent : TEXCOORD5;
    #endif

    half4 fogFactorAndVertexLight : TEXCOORD6; // x: fogFactor, yzw: vertex light

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        float4 shadowCoord : TEXCOORD7;
    #endif
    float4 screenPos : TEXCOORD8;
    float3 originWS : TEXCOORD9;
    float4 spos : TEXCOORD10;
    float3 frontDirectionWS : TEXCOORD11;
    float3 rightDirectionWS : TEXCOORD12;
    float2 faceShadowUV : TEXCOORD13;

    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;
    inputData.positionWS = input.posWS;

    #ifdef _NORMALMAP
        inputData.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
    #else
        inputData.normalWS = input.normal;
    #endif

    #if VERSION_GREATER_EQUAL(10, 0)
        inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
        inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
    #endif

    #ifdef _NORMALMAP
        half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
        inputData.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
    #else
        half3 viewDirWS = input.viewDir;
    #endif

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
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, lerp(inputData.normalWS, TransformObjectToWorldDir(float3(0, 1, 0)), _FlattenGI));
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = normalize(GetCameraPositionWS() - vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.posWS.xyz = vertexInput.positionWS;
    output.positionCS = vertexInput.positionCS;

    #ifdef _NORMALMAP
        output.normal = half4(normalInput.normalWS, viewDirWS.x);
        output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
        output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
    #else
        output.viewDir = viewDirWS;
        output.normal = half4(NormalizeNormalPerVertex(normalInput.normalWS), viewDirWS.x);
        output.bitangent = float4(normalInput.bitangentWS, viewDirWS.z);
    #endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normal.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

    output.screenPos = ComputeScreenPos(output.positionCS);
    output.spos = output.positionCS;
    output.originWS = TransformObjectToWorld(float3(0, 0, 0));
    output.frontDirectionWS = TransformObjectToWorldDir(_FaceFrontDirection);
    output.rightDirectionWS = TransformObjectToWorldDir(_FaceRightDirection);
    output.faceShadowUV = input.faceShadowUV;
    return output;
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

// Used for StandardSimpleLighting shader
half4 LitPassFragmentSimple(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    UnityDither(_DitherThreshold, input.screenPos / input.screenPos.w);

    float2 uv = input.uv;
    half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half3 diffuse = diffuseAlpha.rgb * _BaseColor.rgb;

    half alpha = diffuseAlpha.a * _BaseColor.a;
    AlphaDiscard(alpha, _Cutoff);

    #ifdef _ALPHAPREMULTIPLY_ON
        diffuse *= alpha;
    #endif

    half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    half3 emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
    half4 specular = SampleSpecularSmoothness(uv, alpha, _SpecColor, TEXTURE2D_ARGS(_SpecGlossMap, sampler_SpecGlossMap));
    half smoothness = specular.a;

    input.normal = lerp(input.normal, normalize(float4((input.posWS - _SpherizeNormalOrigin.xyz), 1)), _SpherizeNormalEnabled);
    InputData inputData;
    InitializeInputData(input, normalTS, inputData);
    
    OtoonPBRSurfaceData otoonSurfaceData;
    InitializeOtoonSimpleSurfaceData(otoonSurfaceData);
    otoonSurfaceData.posWS = input.posWS;
    otoonSurfaceData.originPosWS = input.originWS;
    otoonSurfaceData.bitangent = input.bitangent.xyz;
    otoonSurfaceData.frontDirectionWS = input.frontDirectionWS;
    otoonSurfaceData.rightDirectionWS = input.rightDirectionWS;
    otoonSurfaceData.faceShadowMapUV = input.faceShadowUV;

    half2 clipUv = input.spos.xy / input.spos.w;
    half4 cpos = TransformObjectToHClip(float3(0, 0, 0));
    clipUv -= cpos.xy / cpos.w;
    clipUv *= cpos.w / UNITY_MATRIX_P._m11;
    clipUv.x *= _ScreenParams.x / _ScreenParams.y;

    half4 color = UniversalFragmentToon_Extend(inputData, diffuse, specular, smoothness, emission, alpha, input.uv, clipUv, otoonSurfaceData);
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, _Surface);

    return color;
}

#endif
