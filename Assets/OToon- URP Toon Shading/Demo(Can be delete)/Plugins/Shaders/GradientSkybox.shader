// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'
/*
Copyright (C) 2013 Keijiro Takahashi

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
Shader "Skybox/Gradient Skybox"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (1, 1, 1, 0)
        _Color2 ("Color 2", Color) = (1, 1, 1, 0)
        _UpVector ("Up Vector", Vector) = (0, 1, 0, 0)
        _Intensity ("Intensity", Float) = 1.0
        _Exponent ("Exponent", Float) = 1.0
        // The properties below are used in the custom inspector.
        _UpVectorPitch ("Up Vector Pitch", float) = 0
        _UpVectorYaw ("Up Vector Yaw", float) = 0
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    struct appdata
    {
        float4 position : POSITION;
        float3 texcoord : TEXCOORD0;
    };
    
    struct v2f
    {
        float4 position : SV_POSITION;
        float3 texcoord : TEXCOORD0;
    };
    
    half4 _Color1;
    half4 _Color2;
    half4 _UpVector;
    half _Intensity;
    half _Exponent;
    
    v2f vert(appdata v)
    {
        v2f o;
        o.position = UnityObjectToClipPos(v.position);
        o.texcoord = v.texcoord;
        return o;
    }
    
    fixed4 frag(v2f i) : COLOR
    {
        half d = dot(normalize(i.texcoord), _UpVector) * 0.5f + 0.5f;
        return lerp(_Color1, _Color2, pow(d, _Exponent)) * _Intensity;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType" = "Background" "Queue" = "Background" }
        Pass
        {
            ZWrite Off
            Cull Off
            Fog
            {
                Mode Off
            }
            CGPROGRAM

            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma vertex vert
            #pragma fragment frag
            ENDCG

        }
    }
    CustomEditor "GradientSkyboxInspector"
}