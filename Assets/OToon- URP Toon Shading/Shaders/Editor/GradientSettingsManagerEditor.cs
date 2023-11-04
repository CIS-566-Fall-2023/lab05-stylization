using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(GradientSettingsManager))]
public class GradientSettingsManagerEditor : Editor
{
    private static Dictionary<string, GradientModel> m_cached;
    private GUIStyle m_titleStyle;
    private List<Texture2D> m_textures = new List<Texture2D>();
    private bool hasMissingTexture;

    public override void OnInspectorGUI()
    {
        if (m_cached == null)
        {
            ((GradientSettingsManager)target).UpdateDataMap();
            m_cached = ((GradientSettingsManager)target).LatestGradientMap;
        }
        //base.OnInspectorGUI();
        if (m_titleStyle == null)
        {
            m_titleStyle = new GUIStyle("ShurikenModuleTitle");
            m_titleStyle.font = new GUIStyle(EditorStyles.boldLabel).font;
            m_titleStyle.border = new RectOffset(15, 7, 4, 4);
            m_titleStyle.fontSize = 15;
            m_titleStyle.fixedHeight = 28;
            m_titleStyle.contentOffset = new Vector2(20f, -2f);
        }
        var rect = GUILayoutUtility.GetRect(16f, 28f, m_titleStyle);

        GUI.Box(rect, "Referenced Texture Preview", m_titleStyle);
        GUILayout.Label("For user install OToon prior to version 1.5, please hit below button");
        if (GUILayout.Button("Transfer Gradient Data"))
        {
            TransferGradientData();
            EditorUtility.SetDirty(target); //
        }
        GUILayout.Space(30);
        var index = 0;
        if (hasMissingTexture)
        {
            var origin = GUI.backgroundColor;
            GUI.backgroundColor = new Color(3f / 255f, 186f / 255f, 252f / 255f, 1f);
            if (GUILayout.Button("Remove missing texture data"))
            {
                ((GradientSettingsManager)target).ClearMissingData();
                EditorUtility.SetDirty(target); //
            }
            GUI.backgroundColor = origin;
        }
        ReferenceAllDepenedAssets();
        foreach (var key in m_cached.Keys)
        {
            EditorGUILayout.PrefixLabel(key);
            if (m_textures[index] == null)
            {
                var style = new GUIStyle("LargeLabel");
                style.fontSize = 10;
                style.normal.textColor = Color.yellow;
                EditorGUILayout.LabelField("Missing Textures", style);
            }
            else
            {
                EditorGUILayout.ObjectField(m_textures[index], typeof(Texture2D), false);
            }
            index++;
        }
    }

    public void TransferGradientData()
    {
        if (m_cached == null)
        {
            ((GradientSettingsManager)target).UpdateDataMap();
            m_cached = ((GradientSettingsManager)target).LatestGradientMap;
        }
        foreach (var kvp in m_cached)
        {
            var texGUID = kvp.Key;
            var path = AssetDatabase.GUIDToAssetPath(texGUID);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                textureImporter.userData = JsonUtility.ToJson(kvp.Value);
                textureImporter.SaveAndReimport();
            }
        }
    }

    private void ReferenceAllDepenedAssets()
    {
        hasMissingTexture = false;
        m_textures.Clear();
        foreach (var key in m_cached.Keys)
        {
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(key));
            if (tex == null)
            {
                hasMissingTexture = true;
            }
            m_textures.Add(tex);
        }
    }
}