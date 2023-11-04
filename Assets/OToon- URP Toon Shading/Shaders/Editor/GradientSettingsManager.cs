using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

//Legacy class
[Serializable]
public class GradientSettingsManager : AssetSingleton<GradientSettingsManager>
{
    [Serializable]
    private class GradientTextureData
    {
        public string GUID;
        public GradientModel Model;
    };

    [SerializeField]
    private List<GradientTextureData> m_gradientDataList;

    public Dictionary<string, GradientModel> LatestGradientMap = new Dictionary<string, GradientModel>();
    private bool forceUpdate;

    [ContextMenu("Clear data")]
    public void Clean()
    {
        m_gradientDataList.Clear();
        LatestGradientMap.Clear();
    }

    private void GenerateMap()
    {
        LatestGradientMap.Clear();
        for (int i = 0; i < m_gradientDataList.Count; i++)
        {
            LatestGradientMap.Add(m_gradientDataList[i].GUID, m_gradientDataList[i].Model);
        }
        forceUpdate = false;
    }

    public void UpdateDataMap()
    {
        if (LatestGradientMap.Keys.Count != m_gradientDataList.Count || forceUpdate)
            GenerateMap();
    }

    public void ClearMissingData()
    {
        var keysToRemove = new List<string>();
        var modelsToRemove = new List<GradientModel>();
        foreach (var k in LatestGradientMap.Keys)
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(k)) == null)
            {
                keysToRemove.Add(k);
                modelsToRemove.Add(LatestGradientMap[k]);
            }
        }
        foreach (var key in keysToRemove)
        {
            RemoveData(key);
        }
        UpdateDataMap();
    }

    private void RemoveData(string guid)
    {
        m_gradientDataList.RemoveAll(e => e.GUID == guid);
    }

    private void OnValidate()
    {
        GenerateMap();
    }

    public void AddGradientData(string guid, Gradient gradient)
    {
        var index = m_gradientDataList.FindIndex(e => e.GUID == guid);
        if (index == -1)
        {
            m_gradientDataList.Add(new GradientTextureData
            {
                GUID = guid,
                Model = new GradientModel(gradient),
            });
        }
        else
        {
            m_gradientDataList[index].Model = new GradientModel(gradient);
        }
        forceUpdate = true;
        UpdateDataMap();

    }

    public Gradient GetGradientData(string guid)
    {
        UpdateDataMap();
        if (!LatestGradientMap.ContainsKey(guid))
        {
            var defaultGradient = new Gradient();
            defaultGradient.colorKeys = new[] { new GradientColorKey(Color.black, 0f), new GradientColorKey(Color.white, 0.51f) };
            defaultGradient.alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
            return defaultGradient;
        }
        return LatestGradientMap[guid].ToGradient();
    }
}

[Serializable]
public class GradientModel
{
    public GradientMode mode;
    public ColorKey[] colorKeys;
    public AlphaKey[] alphaKeys;

    public GradientModel() { }

    public GradientModel(Gradient source)
    {
        FromGradient(source);
    }

    public void FromGradient(Gradient source)
    {
        mode = source.mode;
        colorKeys = source.colorKeys.Select(key => new ColorKey(key)).ToArray();
        alphaKeys = source.alphaKeys.Select(key => new AlphaKey(key)).ToArray();
    }

    public void ToGradient(Gradient gradient)
    {
        gradient.mode = mode;
        gradient.colorKeys = colorKeys.Select(key => key.ToGradientKey()).ToArray();
        gradient.alphaKeys = alphaKeys.Select(key => key.ToGradientKey()).ToArray();
    }

    public Gradient ToGradient()
    {
        var gradient = new Gradient();
        ToGradient(gradient);
        return gradient;
    }

    [Serializable]
    public struct ColorKey
    {
        public Color color;
        public float time;

        public ColorKey(GradientColorKey source)
        {
            color = default;
            time = default;
            FromGradientKey(source);
        }

        public void FromGradientKey(GradientColorKey source)
        {
            color = source.color;
            time = source.time;
        }

        public GradientColorKey ToGradientKey()
        {
            GradientColorKey key;
            key.color = color;
            key.time = time;
            return key;
        }
    }

    [Serializable]
    public struct AlphaKey
    {
        public float alpha;
        public float time;

        public AlphaKey(GradientAlphaKey source)
        {
            alpha = default;
            time = default;
            FromGradientKey(source);
        }

        public void FromGradientKey(GradientAlphaKey source)
        {
            alpha = source.alpha;
            time = source.time;
        }

        public GradientAlphaKey ToGradientKey()
        {
            GradientAlphaKey key;
            key.alpha = alpha;
            key.time = time;
            return key;
        }
    }
}