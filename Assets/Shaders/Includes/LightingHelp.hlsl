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

void ChooseColor_float(float3 Highlight, float3 Shadow, float Diffuse, float Threshold, out float3 OUT)
{
    if (Diffuse < Threshold)
    {
        OUT = Shadow;
    }
    else
    {
        OUT = Highlight;
    }
}

void ChooseColor3Tone_float(float3 Highlight, float3 Shadow, float3 MidTone, float Diffuse, float Threshold, float Threshold2, out float3 OUT, out float3 SHADOW_HATCH_MASK, out float3 MIDTONE_HATCH_MASK)
{
    SHADOW_HATCH_MASK = float3(0.0f, 0.0f, 0.0f);
    MIDTONE_HATCH_MASK = float3(0.0f, 0.0f, 0.0f);
    if (Diffuse < Threshold && Diffuse < Threshold2)
    {
        OUT = Shadow;
        SHADOW_HATCH_MASK = float3(1.0f, 1.0f, 1.0f);
    }
    else if (Diffuse < Threshold2)
    {
        OUT = MidTone;
        MIDTONE_HATCH_MASK = float3(1.0f, 1.0f, 1.0f);
    }
    else
    {
        OUT = Highlight;
    }
}
