using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private const string folderPath = "Assets/OToon- URP Toon Shading/Data/";
    private static string GetFilePath()
    {
        return folderPath + typeof(T).Name + ".asset";
    }
    static T s_Instance;

    public static T instance
    {
        get
        {
            if (s_Instance == null)
                CreateAndLoad();

            return s_Instance;
        }
    }

    private static void CreateAndLoad()
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(GetFilePath());
        if (asset == null)
        {
            var filePath = GetFilePath();
            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            T t = CreateInstance<T>();
            AssetDatabase.CreateAsset(t, filePath);
            AssetDatabase.SaveAssets();
            s_Instance = t;
        }
        else
        {
            s_Instance = AssetDatabase.LoadAssetAtPath<T>(GetFilePath());
        }
    }

}