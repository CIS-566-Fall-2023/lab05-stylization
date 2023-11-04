using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class GradientExDrawer : MaterialPropertyDrawer
{
    public static readonly GUIContent RampMap = new GUIContent("Ramp Map",
               "Ramp texture to control lighting strengh / color");
    private int resolution;
    private Gradient cachedGradient;
    private string cachedGradientName;

    public GradientExDrawer()
    {
        resolution = 256;
    }

    public GradientExDrawer(float res)
    {
        resolution = (int)res;
    }

    public string TextureName(MaterialProperty prop) => $"{prop.name}";
    private bool hasChanged;

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        //  EditorGUI.PropertyField(position, prop);
        if (prop.type != MaterialProperty.PropType.Texture)
        {
            Debug.LogError($"[GradientEx] used on non-texture property \"{prop.name}\"");
            return;
        }
        var textureName = TextureName(prop);
        EditorGUILayout.BeginHorizontal();
        editor.TexturePropertySingleLine(RampMap, prop);
        if (prop.textureValue != null)
        {
            var originColor = GUI.backgroundColor;
            GUI.backgroundColor = !hasChanged || cachedGradientName != prop.textureValue.name ? originColor : Color.red;
            if (GUILayout.Button("Apply Gradient To File"))
            {
                if (prop.textureValue.name == cachedGradientName)
                {
                    hasChanged = false;
                    var path = AssetDatabase.GetAssetPath(prop.textureValue);
                    var textureAsset = GetTextureAsset(path);
                    File.WriteAllBytes(path, textureAsset.EncodeToPNG());
                    var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                }
            }
            GUI.backgroundColor = originColor;
        }
        else
        {
            GUILayout.Space(15);
            if (GUILayout.Button("New"))
            {
                var defaultGradient = new Gradient();
                defaultGradient.colorKeys = new[] { new GradientColorKey(Color.black, 0.49f), new GradientColorKey(Color.white, 0.5f) };
                defaultGradient.alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

                var path = EditorUtility.SaveFilePanel("Create New Ramp Texture", Application.dataPath, prop.targets[0].name + prop.name, "png");
                if (!string.IsNullOrEmpty(path))
                {
                    var filePath = path.Replace(Application.dataPath, "Assets");
                    var tex = CreateTexture(filePath, prop.targets[0].name + prop.name);
                    File.WriteAllBytes(path, tex.EncodeToPNG());

                    AssetDatabase.ImportAsset(filePath);
                    var ti = AssetImporter.GetAtPath(filePath) as TextureImporter;
                    ti.wrapMode = TextureWrapMode.Clamp;
                    ti.isReadable = true;
                    ti.textureCompression = TextureImporterCompression.Uncompressed;
                    ti.textureFormat = TextureImporterFormat.RGBA32;
                    ti.userData = Encode(defaultGradient);
                    ti.SaveAndReimport();

                    var tex2d = GetTextureAsset(filePath);
                    GradientToTexture(defaultGradient, tex2d);
                    prop.textureValue = tex2d;
                    ApplyGradientToTexture(prop, defaultGradient);
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        Gradient currentGradient = null;
        if (prop.targets.Length == 1)
        {
            if ((prop.textureValue) != null)
            {
                currentGradient = Decode(prop.textureValue);
            }
            if (currentGradient == null)
            {
                currentGradient = new Gradient() { };
            }

            EditorGUI.showMixedValue = false;
        }
        else
        {
            EditorGUI.showMixedValue = true;
        }
        EditorGUI.BeginDisabledGroup(prop.textureValue == null);
        using (var changeScope = new EditorGUI.ChangeCheckScope())
        {
            currentGradient = EditorGUILayout.GradientField("Gradient Values", currentGradient);

            if (changeScope.changed)
            {

                cachedGradientName = prop.textureValue.name;
                hasChanged = true;
                ApplyGradientToTexture(prop, currentGradient);
                var path = AssetDatabase.GetAssetPath(prop.textureValue);
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                ti.userData = Encode(currentGradient);
            }
        }
        EditorGUI.EndDisabledGroup();


        EditorGUI.showMixedValue = false;
    }

    private void ApplyGradientToTexture(MaterialProperty prop, Gradient currentGradient)
    {
        foreach (Object target in prop.targets)
        {
            var path = AssetDatabase.GetAssetPath(prop.textureValue);
            var textureAsset = GetTextureAsset(path);
            Undo.RecordObject(textureAsset, "Change Material Gradient");
            GradientToTexture(currentGradient, textureAsset);
            EditorUtility.SetDirty(textureAsset);
            var material = (Material)target;
            material.SetTexture(prop.name, textureAsset);
        }
    }

    private Texture2D CreateTexture(string path, string name = "unnamed texture")
    {
        var textureAsset = new Texture2D(resolution, 4, TextureFormat.ARGB32, false);
        textureAsset.wrapMode = TextureWrapMode.Clamp;
        textureAsset.name = name;
        AssetDatabase.ImportAsset(path);
        return textureAsset;
    }

    private string Encode(Gradient gradient)
    {
        var jsonString = JsonUtility.ToJson(new GradientModel(gradient));
        return jsonString;
    }

    private Gradient Decode(Texture texture)
    {
        try
        {
            var json = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)).userData;
            return JsonUtility.FromJson<GradientModel>(json).ToGradient();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private Texture2D GetTextureAsset(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    private void GradientToTexture(Gradient gradient, Texture2D texture)
    {
        if (gradient == null)
            return;
        for (int x = 0; x < texture.width; x++)
        {
            var color = gradient.Evaluate((float)x / (texture.width - 1));
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
    }
}