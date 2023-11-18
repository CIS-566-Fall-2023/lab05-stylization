
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
public class SplitDrawer : MaterialPropertyDrawer
{
    private string[] PropNames;
    private Dictionary<string, Vector2> MinMaxTable;
    public SplitDrawer(string a, string b, string c, string d, float aMin, float aMax, float bMin, float bMax, float cMin, float cMax, float dMin, float dMax)
    {
        PropNames = new string[] { "", "", "", "" };
        MinMaxTable = new Dictionary<string, Vector2>();
        PropNames[0] = a;
        PropNames[1] = b;
        PropNames[2] = c;
        PropNames[3] = d;
        MinMaxTable.Add(a, new Vector2(aMin, aMax));
        MinMaxTable.Add(b, new Vector2(bMin, bMax));
        MinMaxTable.Add(c, new Vector2(cMin, cMax));
        MinMaxTable.Add(d, new Vector2(dMin, dMax));
    }

    public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
    {
        if (prop.type != MaterialProperty.PropType.Vector)
        {
            Debug.LogWarning(prop.name + " is not a supported type for Split Material Property");
        }
        var vals = prop.vectorValue;
        EditorGUI.indentLevel++;
        EditorGUI.BeginChangeCheck();
        for (int i = 1; i < 5; i++)
        {
            EditorGUILayout.BeginHorizontal();
            var name = PropNames[i - 1];
            EditorGUILayout.PrefixLabel(name);
            if (i == 1)
                vals.x = EditorGUILayout.Slider(vals.x, MinMaxTable[name].x, MinMaxTable[name].y);
            else if (i == 2)
                vals.y = EditorGUILayout.Slider(vals.y, MinMaxTable[name].x, MinMaxTable[name].y);
            else if (i == 3)
                vals.z = EditorGUILayout.Slider(vals.z, MinMaxTable[name].x, MinMaxTable[name].y);
            else if (i == 4)
                vals.w = EditorGUILayout.Slider(vals.w, MinMaxTable[name].x, MinMaxTable[name].y);

            EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;
        EditorGUI.EndChangeCheck();
        prop.vectorValue = vals;
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 6;
    }
}