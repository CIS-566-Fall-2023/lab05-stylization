using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGUI;
using System.Collections.Generic;
using UnityEngine.Rendering;
using OToon;
using UnityEngine.Rendering.Universal;
using System.Linq;

public class OtoonShaderGraph : StandardBaseShaderGUI
{
    public enum PatternSampleSpace
    {
        ObjectSpace,
        ScreenSpace,
    }

    // Properties
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
    private OToon.SavedInt m_patternSampleSpaceValue;
    private MaterialProperty m_halfToneEnabledProp;
    private MaterialProperty m_halfToneShapeProp;
    private MaterialProperty m_hatchingEnabledProp;
    private MaterialProperty m_RimLightEnabledProp;
    private MaterialProperty m_SpecLightEnabledProp;
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
    private PatternSampleSpace m_patternSampleSpace;

    public override void FindProperties(MaterialProperty[] properties)
    {
        base.FindProperties(properties);
        m_RampColorProp = BaseShaderGUI.FindProperty("_USERAMPCOLOR", properties, false);
        m_RimLightEnabledProp = BaseShaderGUI.FindProperty("_RIMLIGHTING", properties, false);
        m_SpecLightEnabledProp = BaseShaderGUI.FindProperty("_SPECULARHIGHTLIGHTS", properties, false);
        m_halfToneEnabledProp = BaseShaderGUI.FindProperty("_HalfToneEnabled", properties, false);
        m_halfToneShapeProp = BaseShaderGUI.FindProperty("_HAFTONESHAPES", properties, false);
        m_hatchingEnabledProp = BaseShaderGUI.FindProperty("_HatchingEnabled", properties, false);
    }

    public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
    {
        this.materialEditor = materialEditorIn;
        FindProperties(properties); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly

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
            m_patternSampleSpaceValue = new OToon.SavedInt($"{m_HeaderStateKey}.patternSampleSpace",
            material.GetFloat("_HalftoneUseScreenSpaceUV") == 0 ? (int)PatternSampleSpace.ObjectSpace : (int)PatternSampleSpace.ScreenSpace);
            m_patternSampleSpace = (PatternSampleSpace)m_patternSampleSpaceValue.value;
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
        m_SpecOptionFoldout.value = materialEditor.Foldout(m_SpecOptionFoldout.value, "Toon Specular Hightlights", material.IsKeywordEnabled("_SPECULARHIGHTLIGHTS"));
        if (m_SpecOptionFoldout.value)
        {
            if (m_SpecLightEnabledProp != null)
            {
                materialEditor.DrawKeywordToggle(m_SpecLightEnabledProp, "_SPECULARHIGHTLIGHTS", "Enable Specular Lighting");
            }
            if (!material.IsKeywordEnabled("_SPECULARHIGHTLIGHTS"))
            {
                GUI.enabled = false;
            }
            foreach (var prop in _properties)
            {
                if (prop.displayName.Contains("[Specular]"))
                {
                    materialEditor.DrawStandard(prop);
                    EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                }
            }
            GUI.enabled = true;
        }
        EditorGUI.indentLevel = originIndentLevel;
        EditorGUILayout.Space();
        //

        m_ToonOptionFoldout.value = materialEditor.Foldout(m_ToonOptionFoldout.value, "Toon Options", true);
        if (m_ToonOptionFoldout.value)
        {

            foreach (var prop in _properties)
            {

                if (prop.displayName.Contains("[AllToon]"))
                {
                    if (prop.displayName.Contains("[Hide]"))
                        continue;
                    if (m_RampColorProp.floatValue == 0 && prop.name.Contains("_OverrideShadowColor"))
                    {
                        continue;
                    }
                    if (prop.displayName.Contains("_RampTex") && material.GetFloat("_StepViaRampTexture") == 0)
                    {
                        continue;
                    }
                    if (prop.displayName.Contains("_RampTex"))
                    {
                        var gradientProp = new GradientExDrawer();
                        gradientProp.OnGUI(EditorGUILayout.GetControlRect(), prop, GradientExDrawer.RampMap, materialEditor);
                    }
                    else
                    {
                        materialEditor.DrawToonStandard(prop, m_RampColorProp, true);
                    }
                    EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                }
            }
        }
        EditorGUI.indentLevel = originIndentLevel;
        EditorGUILayout.Space();


        m_RimOptionFoldout.value = materialEditor.Foldout(m_RimOptionFoldout.value, "Rim Lighting", material.IsKeywordEnabled("_RIMLIGHTING_ON"), material.IsKeywordEnabled("_RIMLIGHTING"), "_RimColor", true);
        if (m_RimOptionFoldout.value)
        {
            if (m_RimLightEnabledProp != null)
            {
                materialEditor.DrawKeywordToggle(m_RimLightEnabledProp, "_RIMLIGHTING_ON", "Enable Rim Lighting");
            }
            if (!material.IsKeywordEnabled("_RIMLIGHTING_ON"))
            {
                GUI.enabled = false;
            }
            foreach (var prop in _properties)
            {
                if (prop.displayName.Contains("[Rim]"))
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
        var overlayColorPropName = m_halfToneEnabledProp.floatValue == 1 ? "_HalftoneColor" : "_HatchingColor";
        m_OverlayOptionFoldout.value = materialEditor.Foldout(m_OverlayOptionFoldout.value, "Halftone / Hatching Overlay", overlayEnabled, overlayEnabled, overlayColorPropName, true);
        if (m_OverlayOptionFoldout.value)
        {
            materialEditor.DrawOverlayModeButtons(m_halfToneEnabledProp, m_hatchingEnabledProp);
            var targetMode = m_halfToneEnabledProp.floatValue == 1 ? "[Halftone]" : "NONE";
            targetMode = m_hatchingEnabledProp.floatValue == 1 ? "[Hatching]" : targetMode;
            if (targetMode != "NONE")
            {
                if (m_halfToneEnabledProp.floatValue == 1)
                {
                    m_patternSampleSpace = (PatternSampleSpace)EditorGUILayout.EnumPopup("Pattern Sample Space", m_patternSampleSpace);
                    if (m_patternSampleSpace == PatternSampleSpace.ObjectSpace)
                    {
                        material.SetFloat("_HalftoneUseScreenSpaceUV", 0);
                    }
                    else
                    {
                        material.SetFloat("_HalftoneUseScreenSpaceUV", 1);
                    }
                }
                if (m_halfToneEnabledProp.floatValue == 1)
                {
                    materialEditor.DrawStandard(m_halfToneShapeProp);
                    EditorGUIHelper.CheckIndentLevel(originIndentLevel, m_halfToneShapeProp);
                }
                foreach (var prop in _properties)
                {
                    if (prop.displayName.Contains("[Hide]"))
                        continue;
                    if (prop.displayName.Contains(targetMode))
                    {
                        materialEditor.DrawStandard(prop);
                        EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                    }
                }
            }
        }
        EditorGUI.indentLevel = originIndentLevel;
        EditorGUILayout.Space();


        m_LightAndShadowOptionFoldout.value = materialEditor.Foldout(m_LightAndShadowOptionFoldout.value, "Light And Shadow", false, true, "_ShadowColor", true);
        if (m_LightAndShadowOptionFoldout.value)
        {
            foreach (var prop in _properties)
            {
                if (prop.displayName.Contains("[Shadow]"))
                {
                    if (prop.name.Contains("ShadowPattern"))
                    {
                        EditorGUI.indentLevel++;
                        EditorGUI.BeginDisabledGroup(material.GetFloat("_UseClipPattern") == 0);
                        materialEditor.DrawStandard(prop);
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.indentLevel--;
                    }
                    else
                        materialEditor.DrawStandard(prop);
                    EditorGUIHelper.CheckIndentLevel(originIndentLevel, prop);
                }
            }
            GUI.enabled = true;
        }
        EditorGUI.indentLevel = originIndentLevel;
        EditorGUILayout.Space();

    }

    public void DrawSurfaceInputs(Material material, MaterialProperty[] _properties)
    {
        base.DrawSurfaceInputs(material);
        //DrawEmissionProperties(material, true);
        DrawTileOffset(materialEditor, baseMapProp);
        foreach (var prop in _properties)
        {
            if (prop.displayName.Contains("[Surface]"))
            {
                if (prop.name.Contains("Bump"))
                {
                    EditorGUI.BeginDisabledGroup(material.GetFloat("_UseNormalMap") == 0);
                    materialEditor.DrawStandard(prop);
                    EditorGUI.EndDisabledGroup();
                }
                else
                    materialEditor.DrawStandard(prop);
            }
        }

    }

    private void ShaderPropertiesGUI(Material material, MaterialProperty[] _properties)
    {
        var originIndentLevel = EditorGUI.indentLevel;
        EditorGUI.BeginChangeCheck();
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
        materialEditor.EnableInstancingField();
        if (EditorGUI.EndChangeCheck())
        {

        }
    }

    public override void MaterialChanged(Material material)
    {

    }
}
