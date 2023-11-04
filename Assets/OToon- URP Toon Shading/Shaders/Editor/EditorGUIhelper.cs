using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace OToon
{
    public static class EditorGUIHelper
    {
        private static Color EnabledColor = new Color(153f / 255f, 204f / 255f, 255f / 255f, 1);
        private static Color SubEnabledColor = new Color(133f / 255f, 255f / 255f, 166f / 255f, 1);
        private static Color SubDisabledColor = new Color(128f / 255f, 128f / 255f, 130f / 255f, 1);

        public static GUILayoutOption[] shortButtonStyle = new GUILayoutOption[] { GUILayout.Width(130) };
        public static void AutoSetUpRendererFeatureButton<T>(this SerializedObject target, string buttonText, UniversalRendererData data, ScriptableRendererFeature feature) where T : ScriptableObject
        {
            if (GUILayout.Button(buttonText, shortButtonStyle))
            {
                if (feature != null)
                {
                    feature.SetActive(true);
                    EditorUtility.SetDirty(data);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    var newRenderFeature = ScriptableObject.CreateInstance<T>() as T;
                    newRenderFeature.name = "Custom Pass";
                    var serializedFeature = new SerializedObject(newRenderFeature);
                    var layerProp = serializedFeature.FindProperty("mask");
                    var eventProp = serializedFeature.FindProperty("Event");
                    layerProp.intValue = 1;
                    eventProp.intValue = 250;
                    EditorUtility.SetDirty(newRenderFeature);
                    serializedFeature.ApplyModifiedProperties();
                    AssetDatabase.AddObjectToAsset(newRenderFeature, data);
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(newRenderFeature, out var guid, out long localId);
                    var rendererFeatures = target.FindProperty("m_RendererFeatures");
                    var rendererFeaturesMap = target.FindProperty("m_RendererFeatureMap");
                    // Grow the list first, then add - that's how serialized lists work in Unity
                    rendererFeatures.arraySize++;
                    SerializedProperty componentProp = rendererFeatures.GetArrayElementAtIndex(rendererFeatures.arraySize - 1);
                    componentProp.objectReferenceValue = newRenderFeature;

                    // Update GUID Map
                    rendererFeaturesMap.arraySize++;
                    SerializedProperty guidProp = rendererFeaturesMap.GetArrayElementAtIndex(rendererFeaturesMap.arraySize - 1);
                    guidProp.longValue = localId;
                    EditorUtility.SetDirty(data);
                    EditorUtility.SetDirty(newRenderFeature);
                    target.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                }
            }
        }

        private static int GetDefaultRendererIndex(UniversalRenderPipelineAsset asset)
        {
            return (int)typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(asset);
        }

        public static ScriptableRendererData GetDefaultRenderer()
        {
            if (UniversalRenderPipeline.asset)
            {
                ScriptableRendererData[] rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset)
                        .GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(UniversalRenderPipeline.asset);
                int defaultRendererIndex = GetDefaultRendererIndex(UniversalRenderPipeline.asset);

                return rendererDataList[defaultRendererIndex];
            }
            else
            {
                Debug.LogWarning("No Universal Render Pipeline is currently active.");
                return null;
            }
        }

        public static void ToggleRendererFeature(UniversalRendererData data, ScriptableRendererFeature feature)
        {

            if (feature.isActive)
            {
                if (GUILayout.Button("Turn Off", shortButtonStyle))
                {
                    feature.SetActive(false);
                    EditorUtility.SetDirty(data);
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                if (GUILayout.Button("Turn On", shortButtonStyle))
                {
                    feature.SetActive(true);
                    EditorUtility.SetDirty(data);
                    AssetDatabase.SaveAssets();
                }
            }

        }

        public static bool Foldout(this MaterialEditor materialEditor, bool display, string title, bool contentEnabled = false, bool showColorPreview = false, string colorPropName = "", bool showAlphaValue = false)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(10);
            var origin = GUI.color;
            if (contentEnabled)
                GUI.color = EnabledColor;
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.boldLabel).font;
            style.border = new RectOffset(15, 7, 4, 4);
            if (showColorPreview)
            {
                style.padding = new RectOffset(15, 100, 0, 0);
            }
            style.fontSize = 15;
            style.fixedHeight = 28;
            style.contentOffset = new Vector2(20f, -2f);
            var rect = GUILayoutUtility.GetRect(16f, 28f, style);
            GUI.Box(rect, title, style);
            if (showAlphaValue && showColorPreview && Screen.width > 350)
            {
                var alphaRect = new Rect(Screen.width - 100, rect.y + 4f, 100f, 13f);
                var material = materialEditor.target as Material;
                var previewAlpha = material.GetColor(colorPropName).a;
                GUI.Label(alphaRect, "Alpha: " + (int)(previewAlpha * 100) + " % ");
            }

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }


            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }
            GUI.color = origin;

            if (showColorPreview)
            {
                var material = materialEditor.target as Material;
                var previewColor = material.GetColor(colorPropName);
                var colorPreviewRect = new Rect(toggleRect.x + 15f, toggleRect.y, 13f, 13f);
                var previewColorStyle = new GUIStyle("Grad Up Swatch");
                GUI.color = previewColor;
                GUI.Label(colorPreviewRect, "", previewColorStyle);
                GUI.color = origin;

            }
            return display;
        }

        public static bool FoldoutSubMenu(bool display, string title, bool contentEnabled = false)
        {
            var origin = GUI.color;
            GUI.color = contentEnabled ? SubEnabledColor : SubDisabledColor;
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.boldLabel).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.margin = new RectOffset(30, 7, 0, 0);
            style.padding = new RectOffset(5, 7, 4, 4);
            style.fontSize = 14;
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);
            var e = Event.current;

            var toggleRect = new Rect(rect.x + 5, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }
            GUI.color = origin;

            return display;
        }

        public static void DrawKeywordToggle(this MaterialEditor materialEditor, MaterialProperty prop, string keyword, string display)
        {
            var material = materialEditor.target as Material;
            materialEditor.ShaderProperty(prop, display);
            if (!material.IsKeywordEnabled(keyword) && prop.floatValue == 1f)
            {
                material.EnableKeyword(keyword);
            }
            else if (material.IsKeywordEnabled(keyword) && prop.floatValue == 0f)
            {
                material.DisableKeyword(keyword);
            }
        }

        public static int CheckIndentLevel(int originIndent, MaterialProperty property)
        {
            if (property.displayName.Contains("[Indent]"))
            {
                EditorGUI.indentLevel += 1;
            }
            if (property.displayName.Contains("[ResumeIndent]"))
            {
                EditorGUI.indentLevel -= 1;
            }
            return EditorGUI.indentLevel;
        }

        public static void DrawOverlayModeButtons(this MaterialEditor materialEditor, MaterialProperty haftone, MaterialProperty hatching)
        {
            var buttonWidthOffset = 0.9f / 3f;
            GUILayoutOption[] middleButtonStyle = new GUILayoutOption[] { GUILayout.Width(EditorGUIUtility.currentViewWidth * buttonWidthOffset) };
            var style = new GUIStyle("IN TitleText");
            style.fontSize = 13;
            EditorGUILayout.LabelField("Stylized Overlay Mode", style);
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            var originColor = GUI.backgroundColor;
            GUI.backgroundColor = (haftone.floatValue == 0 && hatching.floatValue == 0) ? EnabledColor : originColor;
            if (GUILayout.Button("Off", middleButtonStyle))
            {
                haftone.floatValue = 0;
                hatching.floatValue = 0;
            }
            GUI.backgroundColor = (haftone.floatValue == 1 && hatching.floatValue == 0) ? EnabledColor : originColor;
            if (GUILayout.Button("Halftone", middleButtonStyle))
            {
                haftone.floatValue = 1;
                hatching.floatValue = 0;
            }
            GUI.backgroundColor = (haftone.floatValue == 0 && hatching.floatValue == 1) ? EnabledColor : originColor;
            if (GUILayout.Button("Hatching", middleButtonStyle))
            {
                haftone.floatValue = 0;
                hatching.floatValue = 1;
            }
            GUI.backgroundColor = originColor;
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
        }

        public static void DrawHalfToneStandard(this MaterialEditor materialEditor, MaterialProperty property)
        {
            var buttonWidthOffset = 0.9f / 3f;
            GUILayoutOption[] middleButtonStyle = new GUILayoutOption[] { GUILayout.Width(EditorGUIUtility.currentViewWidth * buttonWidthOffset) };
            var material = materialEditor.target as Material;
            if (property.name == "_HalfTonePatternMap" && !material.IsKeywordEnabled("_HALFTONESHAPE_CUSTOM"))
                return;
            materialEditor.DrawStandard(property);
        }

        public static void DrawToonStandard(this MaterialEditor materialEditor, MaterialProperty property, MaterialProperty useRampColor, bool showRampColorButton = false)
        {
            var buttonWidthOffset = showRampColorButton ? 0.9f / 3f : 0.9f / 2f;
            GUILayoutOption[] middleButtonStyle = new GUILayoutOption[] { GUILayout.Width(EditorGUIUtility.currentViewWidth * buttonWidthOffset) };
            var material = materialEditor.target as Material;
            if (property.name.Contains("_StepViaRampTexture"))
            {
                var style = new GUIStyle("IN TitleText");
                style.fontSize = 13;
                EditorGUILayout.LabelField("Toon Shading Mode", style);
                var originColor = GUI.backgroundColor;
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.Space(15);
                GUI.backgroundColor = (property.floatValue == 0) && useRampColor.floatValue == 0 ? EnabledColor : originColor;
                if (GUILayout.Button("1 Step Only", middleButtonStyle))
                {
                    property.floatValue = 0;
                    useRampColor.floatValue = 0;
                }
                GUI.backgroundColor = property.floatValue == 1 && useRampColor.floatValue == 0 ? EnabledColor : originColor;
                if (GUILayout.Button("Diffuse Ramp", middleButtonStyle))
                {

                    property.floatValue = 1;
                    useRampColor.floatValue = 0;
                }
                if (showRampColorButton)
                {
                    GUI.backgroundColor = useRampColor.floatValue == 1 ? EnabledColor : originColor;
                    if (GUILayout.Button("Color Ramp", middleButtonStyle))
                    {
                        property.floatValue = 1;
                        useRampColor.floatValue = 1;
                    }
                }
                GUI.backgroundColor = originColor;
                GUILayout.EndHorizontal();
                return;
            }
            materialEditor.DrawStandard(property);
        }

        public static void DrawOutlineProp(this MaterialEditor materialEditor, MaterialProperty property)
        {
            var material = materialEditor.target as Material;
            if (material.GetFloat("_OutlineMode") == 0 && property.displayName.Contains("[1]"))
                return;
            string displayName = property.displayName;
            // Remove everything in square brackets.
            displayName = Regex.Replace(displayName, @" ?\[.*?\]", string.Empty);
            var guiContent = new GUIContent(displayName);
            materialEditor.ShaderProperty(property, guiContent);
        }

        public static void DrawStandard(this MaterialEditor materialEditor, MaterialProperty property)
        {
            if (property.displayName.Contains("[Hide]"))
                return;
            string displayName = property.displayName;
            // Remove everything in square brackets.
            displayName = Regex.Replace(displayName, @" ?\[.*?\]", string.Empty);
            var tooltip = OtoonToolTip.Tips.ContainsKey(property.name) ? OtoonToolTip.Tips[property.name] : "";
            var guiContent = new GUIContent(displayName, tooltip);

            if (property.displayName.Contains("[SinglelineTexture]"))
            {
                GUILayout.Space(5);
                materialEditor.TexturePropertySingleLine(guiContent, property);
            }
            else
            {
                materialEditor.ShaderProperty(property, guiContent);
            }
            if (property.displayName.Contains("[AlphaBlend]") && property.type == MaterialProperty.PropType.Color)
            {
                var alphaBlendColor = property.colorValue;
                alphaBlendColor.a = EditorGUILayout.Slider(displayName + " Alpha Blend", property.colorValue.a, 0f, 1f);
                property.colorValue = alphaBlendColor;
            }
        }

        public static void CheckFoldOut(this MaterialEditor materialEditor, string title, MaterialProperty property, string parentPropName,
         SavedBool foldoutState, Dictionary<string, SavedBool> allStates, ref bool propDrawState, Action onDraw)
        {
            if (property.name == parentPropName)
            {
                EditorGUILayout.Space(5);
                if (!allStates.ContainsKey(property.name))
                {
                    allStates.Add(property.name, foldoutState);
                }
                foldoutState.value = FoldoutSubMenu(foldoutState.value, title, property.textureValue != null);
                EditorGUI.indentLevel++;
            }
            foreach (var kvp in allStates)
            {
                if (property.name == kvp.Key || property.displayName.Contains(kvp.Key))
                {
                    var originColor = GUI.contentColor;
                    var guiEnabled = GUI.enabled;
                    if (guiEnabled && property.name != parentPropName)
                    {
                        GUI.contentColor = (materialEditor.target as Material).GetTexture(parentPropName) != null ? originColor : SubDisabledColor;
                        //GUI.enabled = (materialEditor.target as Material).GetTexture(parentPropName) != null;
                    }
                    propDrawState = true;
                    if (kvp.Value.value)
                    {
                        onDraw.Invoke();
                    }
                    // GUI.enabled = guiEnabled;
                    GUI.contentColor = originColor;
                    break;
                }
            }
        }

    }

}