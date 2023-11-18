/*
Sprint Bone Source : https://github.com/yangrc1234/SpringBone

MIT License

Copyright (c) 2017 

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(SpringBone))]
public class SpringBoneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var t = target as SpringBone;
        var so = new SerializedObject(t);
        EditorGUILayout.PropertyField(so.FindProperty("springEnd"));

        EditorGUILayout.HelpBox("If you have don't have other(e.g. Animator) controlling rotation of this gameobject, enable this to fix its rotation. Otherwise don't use it.", MessageType.Info);
        EditorGUILayout.PropertyField(so.FindProperty("useSpecifiedRotation"), new GUIContent("Use custom rotation?"));
        if (t.useSpecifiedRotation)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(so.FindProperty("customRotation"));
            if (GUILayout.Button("Copy current rotation"))
            {
                t.customRotation = t.transform.localRotation.eulerAngles;
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.LabelField("Forces");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(so.FindProperty("stiffness"));
        EditorGUILayout.PropertyField(so.FindProperty("bounciness"));
        EditorGUILayout.PropertyField(so.FindProperty("dampness"));
        EditorGUI.indentLevel--;
        so.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        var t = target as SpringBone;
        var so = new SerializedObject(t);
        Handles.DrawDottedLine(t.transform.position, t.transform.TransformPoint(t.springEnd), 4.0f);
        var currentPos = t.transform.TransformPoint(t.springEnd);
        var size = HandleUtility.GetHandleSize(currentPos) * 0.2f;
        EditorGUI.BeginChangeCheck();
        var fmh_72_59_638347181710768431 = Quaternion.identity; var movedPos = Handles.FreeMoveHandle(currentPos, size, Vector3.one * 0.5f, Handles.SphereHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            so.FindProperty("springEnd").vector3Value =
                    t.transform.InverseTransformPoint(movedPos);
            so.ApplyModifiedProperties();
        }
    }
}