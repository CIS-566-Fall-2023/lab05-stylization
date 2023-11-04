using UnityEngine;
using UnityEditor;

public static class OtoonUpdateHelper
{
    private const string filePath = "Assets/OToon- URP Toon Shading/Data/GradientSettingsManager.asset";

    [InitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {
        var gsm = AssetDatabase.LoadAssetAtPath<GradientSettingsManager>(filePath);
        if (gsm != null)
        {
            EditorApplication.ExecuteMenuItem("Tools/OToon/Legacy Gradient Data Transfer Wizard");
        }
    }
}
