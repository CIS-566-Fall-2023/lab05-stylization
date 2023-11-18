using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGUI;
using System.Collections.Generic;
using UnityEngine.Rendering;
using OToon;
using UnityEngine.Rendering.Universal;

class OToonCustomLitShader : StandardBaseShaderGUI
{
    // Properties
    private SimpleLitGUI.SimpleLitProperties shadingModelProperties;
    private Material _material;

    private OToon.SavedBool m_surfaceOptionFoldout;
    private OToon.SavedBool m_surfaceInputFoldout;
    private OToon.SavedBool m_SpecOptionFoldout;
    private OToon.SavedBool m_SpecClipMapFoldout;
    private OToon.SavedBool m_ToonOptionFoldout;
    private OToon.SavedBool m_DiffuseWrapNoiseFoldout;
    private OToon.SavedBool m_RimOptionFoldout;
    private OToon.SavedBool m_OverlayOptionFoldout;
    private OToon.SavedBool m_OutlineOptionFoldout;
    private OToon.SavedBool m_advanceOptionFoldout;
    private OToon.SavedBool m_LightAndShadowOptionFoldout;
    private OToon.SavedBool m_HairOptionFoldout;
    private OToon.SavedBool m_FaceShadowMapFoldout;
    private OToon.SavedBool m_HairSpecularFoldout;
    private MaterialProperty m_halfToneEnabledProp;
    private MaterialProperty m_hatchingEnabledProp;
    private MaterialProperty m_RimLightEnabledProp;
    private MaterialProperty m_ToonEnabledProp;
    private MaterialProperty m_RampColorProp;
    private MaterialProperty m_OutlineEnabledProp;
    private MaterialProperty m_OutlineModeProp;
    private MaterialProperty m_HairProp;
    private MaterialProperty m_SpherizeNormalProp;
    private MaterialProperty m_FaceShadowMapEnabledProp;
    private const string k_KeyPrefix = "OToon:Material:UI_State:";
    private string m_HeaderStateKey = null;

    private UniversalRendererData m_forwardRendererData;
    private SerializedObject m_forwardRendererSerilizedData;
    private Dictionary<string, SavedBool> m_foldOutStates;

    public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
    {
        if (_material == null)
            _material = materialEditorIn.target as Material;
        if (materialEditorIn == null)
            throw new ArgumentNullException("materialEditorIn");

        FindProperties(properties); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
        materialEditor = materialEditorIn;
        Material material = materialEditor.target as Material;
        // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
        // material to a universal shader.
        if (m_FirstTimeApply)
        {
            m_HeaderStateKey = k_KeyPrefix + material.shader.name; // Create key string for editor prefs
            m_surfaceOptionFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.surfaceOptionFoldout", false);
            m_surfaceInputFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.surfaceInputFoldout", false);
            m_SpecOptionFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.specOptionFoldout", false);
            m_SpecClipMapFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.specClipMapOptionFoldout", false);
            m_ToonOptionFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.toonOptionFoldout", false);
            m_DiffuseWrapNoiseFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.toonWrapNoiseOptionFoldout", false);
            m_RimOptionFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.rimOptionFoldout", false);
            m_OverlayOptionFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.halfToneOptionFoldout", false);
            m_advanceOptionFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.advanceOptionFoldout", false);
            m_OutlineOptionFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.outlineOptionFoldout", false);
            m_LightAndShadowOptionFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.lightAndShadowOptionFoldout", false);
            m_HairOptionFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.hairOptionFoldout", false);
            m_FaceShadowMapFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.faceShadowMapFoldout", false);
            m_HairSpecularFoldout = new OToon.SavedBool($"{m_HeaderStateKey}.hairSpecularFoldout", false);

            OnOpenGUI(material, materialEditorIn);
            m_FirstTimeApply = false;
        }
        if (m_foldOutStates == null)
        {
            m_foldOutStates = new Dictionary<string, SavedBool>();
        }

        ShaderPropertiesGUI(material, properties);
    }

    private void DrawOToonProperties(Material material, MaterialProperty[] _properties)
    {
        var originIndentLevel = EditorGUI.indentLevel;
        var currentIndent = originIndentLevel;
        SimpleLitGUI.SpecularSource specularSource = (SimpleLitGUI.SpecularSource)shadingModelProperties.specHighlights.floatValue;
        m_SpecOptionFoldout.value = materialEditor.Foldout(m_SpecOptionFoldout.value, "Toon Specular Hightlights", specularSource == SimpleLitGUI.SpecularSource.SpecularTextureAndColor, specularSource == SimpleLitGUI.SpecularSource.SpecularTextureAndColor, "_SpecColor");
        if (m_SpecOptionFoldout.value)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = shadingModelProperties.specHighlights.hasMixedValue;
            bool enabledSpec = EditorGUILayout.Toggle("Enable Specular Hightlight", specularSource == SimpleLitGUI.SpecularSource.SpecularTextureAndColor);
            if (EditorGUI.EndChangeCheck())
                shadingModelProperties.specHighlights.floatValue = enabledSpec ? (float)SimpleLitGUI.SpecularSource.SpecularTextureAndColor : (float)SimpleLitGUI.SpecularSource.NoSpecular;
            EditorGUI.showMixedValue = false;
            if (!enabledSpec)
            {
                GUI.enabled = false;
            }
            foreach (var prop in _properties)
            {
                var hadDraw = false;
                if (prop.displayName.Contains("[ToonSpec]"))
                {
                    materialEditor.CheckFoldOut("Toon Specular Brush Mask", prop, "_SpecularClipMask", m_SpecClipMapFoldout, m_foldOutStates, ref hadDraw,
                    () =>
                    {
                        materialEditor.DrawStandard(prop);
                    });
                    if (!hadDraw)
                    {
                        materialEditor.DrawStandard(prop);
                        hadDraw = true;
                    }
                    EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                }
            }
            GUI.enabled = true;
        }
        EditorGUI.indentLevel = originIndentLevel;
        EditorGUILayout.Space();


        m_ToonOptionFoldout.value = materialEditor.Foldout(m_ToonOptionFoldout.value, "Toon Options", true);
        if (m_ToonOptionFoldout.value)
        {
            foreach (var prop in _properties)
            {
                var hadDraw = false;
                if (prop.displayName.Contains("[AllToon]"))
                {
                    if (m_RampColorProp.floatValue == 0 && prop.name == "_OverrideShadowColor")
                    {
                        continue;
                    }
                    if (prop.name.Contains("_RampTex") && material.GetFloat("_StepViaRampTexture") == 0)
                    {
                        continue;
                    }
                    materialEditor.CheckFoldOut("Dissfule Wrap Noise", prop, "_DiffuseWrapNoise", m_DiffuseWrapNoiseFoldout, m_foldOutStates, ref hadDraw, () =>
                       {
                           materialEditor.DrawToonStandard(prop, m_RampColorProp);
                       });
                    if (!hadDraw)
                    {
                        materialEditor.DrawToonStandard(prop, m_RampColorProp, true);
                        hadDraw = true;
                    }
                    EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                }
            }
        }
        EditorGUI.indentLevel = originIndentLevel;
        EditorGUILayout.Space();


        m_RimOptionFoldout.value = materialEditor.Foldout(m_RimOptionFoldout.value, "Rim Lighting", m_RimLightEnabledProp.floatValue == 1, m_RimLightEnabledProp.floatValue == 1, "_RimColor", true);
        if (m_RimOptionFoldout.value)
        {
            if (m_RimLightEnabledProp != null)
            {
                materialEditor.DrawKeywordToggle(m_RimLightEnabledProp, "_RIMLIGHTING_ON", "Enable Rim Lighting");
            }
            if (!_material.IsKeywordEnabled("_RIMLIGHTING_ON"))
            {
                GUI.enabled = false;
            }
            foreach (var prop in _properties)
            {
                if (prop.displayName.Contains("[RimLight]"))
                {
                    materialEditor.DrawStandard(prop);
                    EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                }

            }
            GUI.enabled = true;
        }
        EditorGUI.indentLevel = originIndentLevel;
        EditorGUILayout.Space();
        var overlayEnabled = m_halfToneEnabledProp.floatValue == 1 || m_hatchingEnabledProp.floatValue == 1;
        var overlayColorPropName = m_halfToneEnabledProp.floatValue == 1 ? "_HalfToneColor" : "_HatchingColor";
        m_OverlayOptionFoldout.value = materialEditor.Foldout(m_OverlayOptionFoldout.value, "Halftone / Hatching Overlay", overlayEnabled, overlayEnabled, overlayColorPropName, true);
        if (m_OverlayOptionFoldout.value)
        {
            materialEditor.DrawOverlayModeButtons(m_halfToneEnabledProp, m_hatchingEnabledProp);
            var targetMode = m_halfToneEnabledProp.floatValue == 1 ? "[Halftone]" : "NONE";
            targetMode = m_hatchingEnabledProp.floatValue == 1 ? "[hatching]" : targetMode;
            if (targetMode != "NONE")
            {
                foreach (var prop in _properties)
                {
                    if (prop.displayName.Contains(targetMode))
                    {
                        materialEditor.DrawHalfToneStandard(prop);
                        EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                    }
                }
            }
        }
        EditorGUI.indentLevel = originIndentLevel;
        EditorGUILayout.Space();

        m_OutlineOptionFoldout.value = materialEditor.Foldout(m_OutlineOptionFoldout.value, "Outline Options", m_OutlineEnabledProp.floatValue == 1, m_OutlineEnabledProp.floatValue == 1, "_OutlineColor", true);
        if (m_OutlineOptionFoldout.value)
        {
            materialEditor.DrawStandard(m_OutlineModeProp);
            m_forwardRendererData = EditorGUIHelper.GetDefaultRenderer() as UniversalRendererData;
            OutlineObjectFeature outlineFeature = null;

            if (m_forwardRendererData == null)
            {
                EditorGUILayout.LabelField("Setup URP Renderer Data!");
            }
            else
            {
                foreach (var feature in m_forwardRendererData.rendererFeatures)
                {
                    if (feature is OutlineObjectFeature)
                    {
                        outlineFeature = feature as OutlineObjectFeature;
                        break;
                    }
                }
            }

            if (_material.GetFloat("_OutlineMode") == 0)
            {
                var hint = "";
                EditorGUI.BeginDisabledGroup(true);
                m_forwardRendererData = EditorGUILayout.ObjectField("", m_forwardRendererData, typeof(UniversalRendererData), true) as UniversalRendererData;
                EditorGUI.EndDisabledGroup();
                if (m_forwardRendererSerilizedData == null)
                    m_forwardRendererSerilizedData = new SerializedObject(m_forwardRendererData);

                if (m_forwardRendererData != null && outlineFeature == null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Missing Renderer Feature");
                    m_forwardRendererSerilizedData.AutoSetUpRendererFeatureButton<OutlineObjectFeature>("Auto Setup", m_forwardRendererData, outlineFeature);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.BeginDisabledGroup(outlineFeature == null);
            }

            if (m_forwardRendererData != null && outlineFeature != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Normal Extrude Outline Feature");
                EditorGUIHelper.ToggleRendererFeature(m_forwardRendererData, outlineFeature);
                EditorGUILayout.EndHorizontal();
            }

            if (m_OutlineEnabledProp != null)
            {
                materialEditor.DrawKeywordToggle(m_OutlineEnabledProp, "_OUTLINE", "Enable Outline");
            }

            if (!_material.IsKeywordEnabled("_OUTLINE"))
            {
                GUI.enabled = false;
            }
            foreach (var prop in _properties)
            {
                if (prop.displayName.Contains("[Outline]"))
                {
                    materialEditor.DrawOutlineProp(prop);
                    EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                }
            }
            GUI.enabled = true;
            if (_material.GetFloat("_OutlineMode") == 0)
            {
                EditorGUI.EndDisabledGroup();
            }
        }
        EditorGUILayout.Space();
        EditorGUI.indentLevel = originIndentLevel;

        m_LightAndShadowOptionFoldout.value = materialEditor.Foldout(m_LightAndShadowOptionFoldout.value, "Light And Shadow", false, true, "_ShadowColor", true);
        if (m_LightAndShadowOptionFoldout.value)
        {
            foreach (var prop in _properties)
            {
                if (prop.displayName.Contains("[LightAndShadow]"))
                {
                    materialEditor.DrawStandard(prop);
                    EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                }
            }
        }
        EditorGUI.indentLevel = originIndentLevel;
        EditorGUILayout.Space();

        m_HairOptionFoldout.value = materialEditor.Foldout(m_HairOptionFoldout.value, "Face & Hair Options", m_FaceShadowMapEnabledProp.floatValue == 1 || m_HairProp.floatValue == 1 || m_SpherizeNormalProp.floatValue == 1, m_HairProp.floatValue == 1, "_HairSpecColor", true);
        if (m_HairOptionFoldout.value)
        {
            materialEditor.DrawStandard(m_SpherizeNormalProp);
            EditorGUI.BeginDisabledGroup(m_SpherizeNormalProp.floatValue == 0);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Sphere Center : ");
            EditorGUILayout.LabelField("" + material.GetVector("_SpherizeNormalOrigin") + " in world space");
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
            //Spherized normal

            //Face Shadow Map
            if (m_FaceShadowMapEnabledProp != null)
            {
                materialEditor.DrawKeywordToggle(m_FaceShadowMapEnabledProp, "_FACE_SHADOW_MAP", "Enable Face Shadow Map");
            }
            EditorGUI.BeginDisabledGroup(m_FaceShadowMapEnabledProp.floatValue == 0);
            m_FaceShadowMapFoldout.value = m_FaceShadowMapEnabledProp.floatValue == 1;
            foreach (var prop in _properties)
            {
                var hadFaceShadowMapFoldoutDraw = false;
                if (prop.displayName.Contains("[Face]"))
                {
                    materialEditor.CheckFoldOut("Face Shadow Map", prop, "_FaceShadowMap", m_FaceShadowMapFoldout, m_foldOutStates, ref hadFaceShadowMapFoldoutDraw,
                     () =>
                     {
                         materialEditor.DrawStandard(prop);
                     });
                    if (!hadFaceShadowMapFoldoutDraw)
                    {
                        materialEditor.DrawStandard(prop);
                        hadFaceShadowMapFoldoutDraw = true;
                    }
                    EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                }
            }
            EditorGUI.EndDisabledGroup();
            //Face Shadow Map

            //Hair Specular
            materialEditor.DrawStandard(m_HairProp);
            EditorGUI.BeginDisabledGroup(m_HairProp.floatValue == 0);
            m_HairSpecularFoldout.value = m_HairProp.floatValue == 1;
            foreach (var prop in _properties)
            {
                var hadHairSpecularFoldoutDraw = false;
                if (prop.displayName.Contains("[Hair]"))
                {
                    materialEditor.CheckFoldOut("Hair Specular Light", prop, "_HairSpecNoiseMap", m_HairSpecularFoldout, m_foldOutStates, ref hadHairSpecularFoldoutDraw,
                    () =>
                    {
                        materialEditor.DrawStandard(prop);
                    });
                    if (!hadHairSpecularFoldoutDraw)
                    {
                        materialEditor.DrawStandard(prop);
                        hadHairSpecularFoldoutDraw = true;
                    }
                    EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                }
            }
            EditorGUI.EndDisabledGroup();
            //Hair Specular
        }
        EditorGUI.indentLevel = originIndentLevel;
        EditorGUILayout.Space();

    }

    // collect properties from the material properties
    public override void FindProperties(MaterialProperty[] properties)
    {
        base.FindProperties(properties);
        shadingModelProperties = new SimpleLitGUI.SimpleLitProperties(properties);
        m_RampColorProp = BaseShaderGUI.FindProperty("_UseRampColor", properties, false);
        m_ToonEnabledProp = BaseShaderGUI.FindProperty("_ToonEnabled", properties, false);
        m_RimLightEnabledProp = BaseShaderGUI.FindProperty("_RimEnabled", properties, false);
        m_halfToneEnabledProp = BaseShaderGUI.FindProperty("_HalfToneEnabled", properties, false);
        m_hatchingEnabledProp = BaseShaderGUI.FindProperty("_HatchingEnabled", properties, false);
        m_OutlineEnabledProp = BaseShaderGUI.FindProperty("_OutlineEnabled", properties, false);
        m_OutlineModeProp = BaseShaderGUI.FindProperty("_OutlineMode", properties, false);
        m_HairProp = BaseShaderGUI.FindProperty("_EnabledHairSpec", properties, false);
        m_SpherizeNormalProp = BaseShaderGUI.FindProperty("_SpherizeNormalEnabled", properties, false);
        m_FaceShadowMapEnabledProp = BaseShaderGUI.FindProperty("_FaceShadowMapEnabled", properties, false);
    }

    // material changed check
    public override void MaterialChanged(Material material)
    {
        if (material == null)
            throw new ArgumentNullException("material");

        SetUpLitKeywords(material);
        SimpleLitGUI.SetMaterialKeywords(material);
    }

    private void SetUpLitKeywords(Material material)
    {
        bool alphaClip = material.GetFloat("_AlphaClip") == 1;
        if (alphaClip)
        {
            material.EnableKeyword("_ALPHATEST_ON");
        }
        else
        {
            material.DisableKeyword("_ALPHATEST_ON");
        }

        var queueOffset = 0; // queueOffsetRange;
        var queueOffsetRange = 50;
        if (material.HasProperty("_QueueOffset"))
            queueOffset = queueOffsetRange - (int)material.GetFloat("_QueueOffset");

        SurfaceType surfaceType = (SurfaceType)material.GetFloat("_Surface");
        if (surfaceType == SurfaceType.Opaque)
        {
            if (alphaClip)
            {
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                material.SetOverrideTag("RenderType", "TransparentCutout");
            }
            else
            {
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                material.SetOverrideTag("RenderType", "Opaque");
            }
            material.renderQueue += queueOffset;
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.SetShaderPassEnabled("ShadowCaster", true);
        }
        else
        {
            BlendMode blendMode = (BlendMode)material.GetFloat("_Blend");
            var queue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            // Specific Transparent Mode Settings
            switch (blendMode)
            {
                case BlendMode.Alpha:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                case BlendMode.Premultiply:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                case BlendMode.Additive:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    break;
                case BlendMode.Multiply:
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.EnableKeyword("_ALPHAMODULATE_ON");
                    break;
            }
            // General Transparent Material Settings
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_ZWrite", 0);
            material.renderQueue = queue + queueOffset;
            material.SetShaderPassEnabled("ShadowCaster", false);
        }

        if (material.HasProperty("_ReceiveShadows"))
            CoreUtils.SetKeyword(material, "_RECEIVE_SHADOWS_OFF", material.GetFloat("_ReceiveShadows") == 0.0f);
        // Emission
        if (material.HasProperty("_EmissionColor"))
            MaterialEditor.FixupEmissiveFlag(material);
        bool shouldEmissionBeEnabled =
            (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
        if (material.HasProperty("_EmissionEnabled") && !shouldEmissionBeEnabled)
            shouldEmissionBeEnabled = material.GetFloat("_EmissionEnabled") >= 0.5f;
        CoreUtils.SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
        // Normal Map
        if (material.HasProperty("_BumpMap"))
            CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));
        // Shader specific keyword functions
    }

    // material main surface options
    public override void DrawSurfaceOptions(Material material)
    {

        if (material == null)
            throw new ArgumentNullException("material");

        // Use default labelWidth
        EditorGUIUtility.labelWidth = 0f;

        // Detect any changes to the material
        EditorGUI.BeginChangeCheck();
        {
            base.DrawSurfaceOptions(material);
        }
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obj in blendModeProp.targets)
            {
                MaterialChanged((Material)obj);
            }
        }
    }

    private void ShaderPropertiesGUI(Material material, MaterialProperty[] _properties)
    {
        if (material == null)
            throw new ArgumentNullException("material");

        var originIndentLevel = EditorGUI.indentLevel;
        EditorGUI.BeginChangeCheck();
        m_surfaceOptionFoldout.value = materialEditor.Foldout(m_surfaceOptionFoldout.value, "Surface Options");
        if (m_surfaceOptionFoldout.value)
        {
            DrawSurfaceOptions(material);
            EditorGUILayout.Space();
        }
        EditorGUI.indentLevel = originIndentLevel;


        m_surfaceInputFoldout.value = materialEditor.Foldout(m_surfaceInputFoldout.value, "Surface Inputs", false, true, "_BaseColor");
        if (m_surfaceInputFoldout.value)
        {
            DrawSurfaceInputs(material, _properties);
            EditorGUILayout.Space();
        }
        EditorGUI.indentLevel = originIndentLevel;

        //All custom properties
        DrawOToonProperties(material, _properties);

        m_advanceOptionFoldout.value = materialEditor.Foldout(m_advanceOptionFoldout.value, "Advance");
        if (m_advanceOptionFoldout.value)
        {
            foreach (var prop in _properties)
            {
                if (prop.displayName.Contains("[Advance]"))
                {
                    materialEditor.DrawStandard(prop);
                }
            }
            EditorGUILayout.Space();
            //Environment Reflection
            DrawCustomAdvancedOptions(material);
            //OToon has no GPU Instancing Field
            //DrawAdvancedOptions(material);
            OToonDrawQueueOffsetField();
            EditorGUILayout.Space();
        }
        EditorGUI.indentLevel = originIndentLevel;


        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obj in materialEditor.targets)
                MaterialChanged((Material)obj);
        }
    }

    //For URP 7 backward compatability
    private void OToonDrawQueueOffsetField()
    {
        if (queueOffsetProp != null)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = queueOffsetProp.hasMixedValue;
            var queue = EditorGUILayout.IntSlider(Styles.queueSlider, (int)queueOffsetProp.floatValue, -50, 50);
            if (EditorGUI.EndChangeCheck())
                queueOffsetProp.floatValue = queue;
            EditorGUI.showMixedValue = false;
        }
    }

    // material main surface inputs
    public void DrawSurfaceInputs(Material material, MaterialProperty[] _properties)
    {
        base.DrawSurfaceInputs(material);
        SimpleLitGUI.Inputs(shadingModelProperties, materialEditor, material);
        DrawEmissionProperties(material, true);
        DrawTileOffset(materialEditor, baseMapProp);
        foreach (var prop in _properties)
        {
            if (prop.displayName.Contains("[Surface]"))
            {
                materialEditor.DrawStandard(prop);
            }
        }
    }

    private void DrawCustomAdvancedOptions(Material material)
    {

        //SimpleLitGUI.Advanced(shadingModelProperties);
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        if (material == null)
            throw new ArgumentNullException("material");

        // _Emission property is lost after assigning Standard shader to the material
        // thus transfer it before assigning the new shader
        if (material.HasProperty("_Emission"))
        {
            material.SetColor("_EmissionColor", material.GetColor("_Emission"));
        }

        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
        {
            SetupMaterialBlendMode(material);
            return;
        }

        SurfaceType surfaceType = SurfaceType.Opaque;
        BlendMode blendMode = BlendMode.Alpha;
        if (oldShader.name.Contains("/Transparent/Cutout/"))
        {
            surfaceType = SurfaceType.Opaque;
            material.SetFloat("_AlphaClip", 1);
        }
        else if (oldShader.name.Contains("/Transparent/"))
        {
            // NOTE: legacy shaders did not provide physically based transparency
            // therefore Fade mode
            surfaceType = SurfaceType.Transparent;
            blendMode = BlendMode.Alpha;
        }
        material.SetFloat("_Surface", (float)surfaceType);
        material.SetFloat("_Blend", (float)blendMode);

        MaterialChanged(material);
    }
}