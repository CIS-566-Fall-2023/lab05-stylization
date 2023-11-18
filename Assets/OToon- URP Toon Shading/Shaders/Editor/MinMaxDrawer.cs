
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
public class MinMaxDrawer : MaterialPropertyDrawer
{
    private float m_minLimit;
    private float m_maxLimit;
    private string m_nameMax;
    private string m_nameMin;
    private float prevMin;
    private float prevMax;

    public MinMaxDrawer(string nameMin, string nameMax, float range)
    {
        m_minLimit = -range;
        m_maxLimit = range;
        m_nameMin = nameMin;
        m_nameMax = nameMax;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        if (prop.type != MaterialProperty.PropType.Vector)
        {
            Debug.LogWarning(prop.name + " is not a supported type for MinMax Material Property");
        }
        var vals = prop.vectorValue;
        EditorGUI.BeginChangeCheck();
        vals.x = Mathf.Clamp(EditorGUILayout.FloatField(m_nameMin, vals.x), m_minLimit, m_maxLimit);
        vals.y = Mathf.Clamp(EditorGUILayout.FloatField(m_nameMax, vals.y), m_minLimit, m_maxLimit);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(m_nameMin + "/" + m_nameMax);
        var rect = GUILayoutUtility.GetRect(60f, 30f);
        var toggleRect = new Rect(rect.x - 20f, rect.y - 5f, 100f, 30f);
        EditorGUI.LabelField(toggleRect, new GUIContent(" ( " + (int)vals.x + " , " + (int)vals.y + " )"));
        EditorGUILayout.MinMaxSlider(ref vals.x, ref vals.y, m_minLimit, m_maxLimit);
        EditorGUILayout.EndHorizontal();
        if (vals.x + 2 > vals.y)
        {
            vals.x = prevMin;
        }
        if (vals.y - 2 < prevMin)
        {
            vals.y = prevMax;
        }
        prevMin = vals.x;
        prevMax = vals.y;
        EditorGUI.EndChangeCheck();
        prop.vectorValue = vals;
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 6;
    }
}