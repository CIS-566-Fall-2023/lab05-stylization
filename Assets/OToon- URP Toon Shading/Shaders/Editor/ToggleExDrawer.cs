
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public class ToggleExDrawer : MaterialPropertyDrawer
{
    public ToggleExDrawer()
    {
    }

    static bool IsPropertyTypeSuitable(MaterialProperty prop)
    {
        return prop.type == MaterialProperty.PropType.Float || prop.type == MaterialProperty.PropType.Range;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        if (!IsPropertyTypeSuitable(prop))
        {
            Debug.LogWarning(prop.name + " is not a supported type for ToggleEx Material Property");
        }
        var value = (Math.Abs(prop.floatValue) > 0.001f);
        EditorGUI.BeginChangeCheck();
        value = EditorGUI.Toggle(position, label, value);
        if (EditorGUI.EndChangeCheck())
        {
            prop.floatValue = value ? 1.0f : 0f;
        }
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 16;
    }
}