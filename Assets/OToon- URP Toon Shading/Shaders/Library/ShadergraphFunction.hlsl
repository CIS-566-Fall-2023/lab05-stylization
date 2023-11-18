void ObjectNormalToEye_half(half3 normalOS, out half3 Out)
{
    Out = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normalOS));
}

void ObjectNormalToEye_float(half3 normalOS, out half3 Out)
{
    Out = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normalOS));
}