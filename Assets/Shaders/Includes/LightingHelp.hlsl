void GetMainLight_float(float3 WorldPos, out float3 Color, out float3 Direction, out float DistanceAtten, out float ShadowAtten) {
#ifdef SHADERGRAPH_PREVIEW
    Direction = normalize(float3(0.5, 0.5, 0));
    Color = 1;
    DistanceAtten = 1;
    ShadowAtten = 1;
#else
    #if SHADOWS_SCREEN
        float4 clipPos = TransformWorldToClip(WorldPos);
        float4 shadowCoord = ComputeScreenPos(clipPos);
    #else
        float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    #endif

    Light mainLight = GetMainLight(shadowCoord);
    Direction = mainLight.direction;
    Color = mainLight.color;
    DistanceAtten = mainLight.distanceAttenuation;
    ShadowAtten = mainLight.shadowAttenuation;
#endif
}

void ChooseColor_float(float3 Highlight, float3 Midtone, float3 Shadow, float DiffuseM, float DiffuseHS, float Min_Threshold, float Max_Threshold, out float3 OUT)
{
    if (DiffuseM > Min_Threshold && DiffuseM < Max_Threshold) {
        OUT = Midtone;
    } else if (DiffuseHS < Min_Threshold) {
        OUT = Shadow;
    } else if (DiffuseHS > Max_Threshold) {
        OUT = Highlight;
    } else {
        OUT = Midtone;
    }
}