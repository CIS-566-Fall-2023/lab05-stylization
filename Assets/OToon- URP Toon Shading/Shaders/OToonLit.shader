Shader "URP/OToonLit"
{
    Properties
    {
        // Specular vs Metallic workflow
        [HideInInspector] _WorkflowMode ("WorkflowMode", Float) = 1.0

        [MainTexture] _BaseMap ("Albedo", 2D) = "white" { }
        [MainColor] _BaseColor ("Color", Color) = (1, 1, 1, 1)

        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale ("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap ("Metallic", 2D) = "white" { }

        _SpecColor ("Specular", Color) = (0.2, 0.2, 0.2)
        _SpecGlossMap ("Specular", 2D) = "white" { }

        [ToggleOff] _SpecularHighlights ("Specular Highlights", Float) = 1.0
        [ToggleOff] _EnvironmentReflections ("Environment Reflections", Float) = 1.0

        _BumpScale ("Scale", Float) = 1.0
        _BumpMap ("Normal Map", 2D) = "bump" { }

        _OcclusionStrength ("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap ("Occlusion", 2D) = "white" { }

        [HDR] _EmissionColor ("Color", Color) = (0, 0, 0)
        _EmissionMap ("Emission", 2D) = "white" { }
        
        _DitherTexelSize ("[Surface]Dither Size", Range(1, 20)) = 1
        _DitherThreshold ("[Surface]Dither Threshold", Range(0, 1)) = 1

        _SpecularSize ("[ToonSpec]Specular Size", Range(0.0, 1.0)) = 1.0
        _SpecularFalloff ("[ToonSpec]Specular Falloff", Range(0.0, 1.0)) = 1.0
        _SpecularClipMask ("[ToonSpec][SinglelineTexture]Specular Clip Map", 2D) = "clip map" { }
        _SpecClipMaskScale ("[_SpecularClipMask][ToonSpec]Specular Clip Map Scale", float) = 1.0
        _SpecularClipStrength ("[ResumeIndent][_SpecularClipMask][ToonSpec]Specular Clip Strength", Range(0.0, 1.0)) = 0.0
        
        [Toggle(_TOON_SHADING_ON)] _ToonEnabled ("[Indent]Enable Toon Shading", Float) = 0.0
        _ToonBlending ("[AllToon]PBR/Toon Blending", Range(0.0, 1.0)) = 1.0
        [Toggle] _StepViaRampTexture ("[AllToon]Step Via Ramp Texture", Float) = 0
        [ToggleEx] _UseRampColor ("Use Ramp Color", Float) = 0
        [GradientEx]_RampTex ("[AllToon]Ramp Texture", 2D) = "black" { }
        [Space(10)]_DiffuseStep ("[AllToon]Toon Diffuse Offset", Range(-1, 1)) = 0.0
        _DiffuseWrapNoise ("[AllToon][SinglelineTexture]DiffuseWrap Noise", 2D) = "black" { }
        _NoiseScale ("[_DiffuseWrapNoise][AllToon]Noise Scale", float) = 1
        _NoiseStrength ("[ResumeIndent][_DiffuseWrapNoise][AllToon]Noise Strength", Range(0.01, 1.0)) = 0.1
        
        [Toggle(_RIMLIGHTING_ON)] _RimEnabled ("[Indent]Enable Rim Lighting", Float) = 0.0
        _RimPower ("[RimLight]Rim Power", Range(0, 1)) = 0.55
        _RimLightAlign ("[RimLight]Rim Light Align", Range(-1, 1)) = 0
        _RimLightSmoothness ("[RimLight]Rim Light Smoothness", Range(0, 1)) = 0
        [HDR]_RimColor ("[RimLight][AlphaBlend]Rim Color", Color) = (1, 1, 1, 1)
        
        [ToggleEx] _HalfToneEnabled ("[Indent]Enable HalfTone Shading", Float) = 0.0
        [KeywordEnum(Dot, Stripe, Cross, Custom)]_HalfToneShape ("[Halftone]HalfTone Shape Mode", Float) = 0
        [Enum(Object, 0, Screen, 1)]_HalfToneUvMode ("[Halftone]HalfTone UV Mode", Float) = 0
        _HalfToneColor ("[AlphaBlend][Halftone]HalfTone Color", Color) = (0, 0, 0, 1)
        _HalfTonePatternMap ("[Halftone]Halftone Pattern", 2D) = "black" { }
        _HalftoneTilling ("[Halftone]Brush Tilling", Float) = 8
        _HalfToneNoiseMap ("[Halftone]HalfTone Noise", 2D) = "black" { }
        _HalftoneNoiseClip ("[Halftone]Noise Clip Strength", Range(0, 20)) = 0.8
        [Space(10)]_BrushSize ("[Halftone]Brush Size", Range(0, 2)) = 0.8
        _SizeFalloff ("[Halftone]Lighting Size Factor", Range(0, 1)) = 0
        _HalfToneDiffuseStep ("[Halftone]HalfTone Diffuse Offset", Range(-1, 1)) = 0.0
        _HalftoneFadeDistance ("[Halftone]Fade Distance", Range(0, 100)) = 10
        _HalftoneFadeToColor ("[Halftone]Fade To Color", Range(0, 1)) = 0
        _BrushLowerCut ("[Halftone]Brush Lower Cut", Range(0, 0.5)) = 0
        
        [ToggleEx] _HatchingEnabled ("[Indent]Enable HalfTone Shading", Float) = 0.0
        _HatchingColor ("[AlphaBlend][hatching]Hatching Color", Color) = (0, 0, 0, 1)
        _HatchingNoiseMap ("[hatching]Hatching NoiseMap", 2D) = "black" { }
        _HatchingDrawStrength ("[hatching]Hatching Strenth", Range(0, 15)) = 1
        _HatchingDensity ("[hatching]Hatching Density", Range(0, 2)) = 1
        _HatchingSmoothness ("[hatching]Hatching Edge Smoothness", Range(0.01, 1)) = 0.1
        _HatchingDiffuseOffset ("[hatching]Hatching Diffuse Offset", Range(-1, 1)) = 0
        _HatchingRotation ("[hatching]Hatching Rotation", Range(0, 90)) = 0
        [ToggleEx]_HalfToneIncludeReceivedShadow ("[Halftone][hatching]Include Shadow Receiving Area", Float) = 0.0

        [Enum(NormalExtrude, 0)]_OutlineMode ("[Indent]Outline Mode", Float) = 0
        [Toggle(_OUTLINE)] _OutlineEnabled ("[Indent]Enable Outline", Float) = 0.0
        _OutlineColor ("[Outline]Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("[Outline]Outline Width", Range(0, 12)) = 0.0
        [MinMax(Near, Far, 200)]_OutlineDistancFade ("[Outline] Fade outline with near/far distance ", Vector) = (-25, 50, 0, 0)

        [ToggleEx] _SpherizeNormalEnabled ("[Indent]SpherizeNormalEnabled", Float) = 0.0
        _SpherizeNormalOrigin ("_SpherizeNormalOrigin", Vector) = (0, 0, 0, 0)
        
        [Toggle(_FACE_SHADOW_MAP)] _FaceShadowMapEnabled ("Face ShadowMap Enabled", Float) = 0.0
        _FaceShadowMap ("[Face][SinglelineTexture]Face Shadow Map", 2D) = "white" { }
        _FaceShadowMapPow ("[Face][_FaceShadowMap]Face Shadow Map Power", range(0.001, 0.5)) = 0.2
        _FaceShadowSmoothness ("[Face][_FaceShadowMap]Face Shadow Smoothness", range(0.0, 0.5)) = 0.1
        [Space(30)]_FaceFrontDirection ("[Face][_FaceShadowMap]Face Front Direction", Vector) = (0, 0, 1, 0)
        _FaceRightDirection ("[ResumeIndent][Face][_FaceShadowMap]Face Right Direction", Vector) = (1, 0, 0, 0)
        [Toggle]_EnabledHairSpec ("[Indent]Enable Hair Specular(天使の輪)", float) = 0
        _HairSpecNoiseMap ("[Hair]Noise Map", 2D) = "Noise Map" { }
        _HairSpecColor ("[Hair][AlphaBlend][_HairSpecNoiseMap]Hair Spec Color", Color) = (0, 0, 0)
        _HairSpecNoiseStrength ("[Hair][_HairSpecNoiseMap]Hair Spec Noise Strength", Range(-10, 10)) = 1
        _HairSpecExponent ("[Hair][_HairSpecNoiseMap]Spec Exponent", Range(2, 250)) = 128
        _HairSpecularSize ("[Hair][_HairSpecNoiseMap]Hair Spec Size", Range(0.1, 1.0)) = 0.8
        _HairSpecularSmoothness ("[Hair][_HairSpecNoiseMap]Hair Spec Smoothness", Range(0.1, 1.0)) = 0.1

        _ShadowColor ("[LightAndShadow][AlphaBlend]Shadow Color", Color) = (0, 0, 0, 1)
        _SpecShadowStrength ("[LightAndShadow]Shadow Specular Mask", Range(0, 1)) = 1

        _StencilRef ("[Advance]_StencilRef", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("[Advance]_StencilComp (default = Disable)", Float) = 0

        // Blending state
        [HideInInspector] _Surface ("__surface", Float) = 0.0
        [HideInInspector] _Blend ("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip ("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
        [HideInInspector] _Cull ("__cull", Float) = 2.0

        _ReceiveShadows ("Receive Shadows", Float) = 1.0
        // Editmode props
        [HideInInspector] _QueueOffset ("Queue offset", Float) = 0.0

        // ObsoleteProperties
        [HideInInspector] _MainTex ("BaseMap", 2D) = "white" { }
        [HideInInspector] _Color ("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _GlossMapScale ("Smoothness", Float) = 0.0
        [HideInInspector] _Glossiness ("Smoothness", Float) = 0.0
        [HideInInspector] _GlossyReflections ("EnvironmentReflections", Float) = 0.0

        [HideInInspector][NoScaleOffset]unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" { }
        [HideInInspector][NoScaleOffset]unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" { }
        [HideInInspector][NoScaleOffset]unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" { }
    }

    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True" "ShaderModel" = "4.5" }
        LOD 300

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "OutlineObject" "RenderType" = "Opaque" }
            Cull Front
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma shader_feature_local _OUTLINE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Library/OToonOutline.hlsl"
            ENDHLSL

        }

        // ------------------------------------------------------------------
        //  Forward pass. Shades all light in a single pass. GI + emission + Fog
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Stencil
            {
                Ref[_StencilRef]
                Comp [_StencilComp]
                Pass Replace
            }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _FACE_SHADOW_MAP
            #pragma shader_feature_local _OUTLINE
            #pragma shader_feature_local _HALFTONESHAPE_DOT _HALFTONESHAPE_STRIPE _HALFTONESHAPE_CROSS  _HALFTONESHAPE_CUSTOM
            #pragma shader_feature_local _RIMLIGHTING_ON
            #pragma shader_feature_local _TOON_SHADING_ON
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _OCCLUSIONMAP
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            #include "Library/OToonLitInput.hlsl"
            #include "Library/OToonLitForwardPass.hlsl"
            ENDHLSL

        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Library/OToonLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL

        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Library/OToonLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL

        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMeta

            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECGLOSSMAP

            #include "Library/OToonLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitMetaPass.hlsl"

            ENDHLSL

        }
        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON

            #include "Library/OToonLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Utils/Universal2D.hlsl"
            ENDHLSL

        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "OToonLitShader"
}