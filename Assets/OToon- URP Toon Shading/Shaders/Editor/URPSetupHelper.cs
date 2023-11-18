using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using OToon;
using UnityEngine.Rendering;

public class URPSetupHelper : EditorWindow
{
    public RenderPipelineAsset defaultRenderPipelineAsset;
    private UniversalRendererData m_forwardRendererData;
    private static GUIStyle errorStyle;
    private static GUIStyle correctStyle;
    private static ListRequest ListRequest;
    private static bool m_packgeChecked;
    private static bool m_urpInstalled;
    private static bool m_pipelineAssetInstalled;

    private const string URP_PACKAGE = "com.unity.render-pipelines.universal";
    private const string SRP_CORE_PACKAGE = "com.unity.render-pipelines.core";
    private static List<string> m_installedPackge = new List<string>();

    [MenuItem("Tools/URPSetupHelper")]
    static void URPHelperWindow()
    {
        URPSetupHelper window = ScriptableObject.CreateInstance(typeof(URPSetupHelper)) as URPSetupHelper;
        window.ShowUtility();
        window.minSize = new Vector2(450, 300);
        m_urpInstalled = false;
        m_pipelineAssetInstalled = false;
        CheckPackageList();
    }

    private static void CheckPackageList()
    {
        m_packgeChecked = false;
        ListRequest = Client.List();    // List packages installed for the project
        EditorApplication.update += CheckPackageProgress;
    }


    private static void CheckPackageProgress()
    {
        m_installedPackge.Clear();
        if (ListRequest.IsCompleted)
        {
            if (ListRequest.Status == StatusCode.Success)
                foreach (var package in ListRequest.Result)
                    m_installedPackge.Add(package.name);
            else if (ListRequest.Status >= StatusCode.Failure)
                Debug.Log(ListRequest.Error.message);

            EditorApplication.update -= CheckPackageProgress;
            m_packgeChecked = true;
        }

        if (m_installedPackge.Contains(URP_PACKAGE))
        {
            m_urpInstalled = true;
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("The Otoon shaders will only work in Universal render pipeline. \nMake sure you install URP via package manager and setup the render pipeline asset first", GUILayout.Height(60));
        errorStyle = new GUIStyle();
        errorStyle.normal.textColor = Color.red;
        correctStyle = new GUIStyle();
        correctStyle.normal.textColor = Color.green;

        if (!m_packgeChecked)
        {
            EditorGUILayout.LabelField("Fetching Package Data, Please Wait...");
        }
        else
        {
            if (m_urpInstalled)
            {
                EditorGUILayout.LabelField("Universal RP package has been installed", correctStyle);
            }
            else
            {
                EditorGUILayout.LabelField("Universal RP package not installed, would you like to install it?", errorStyle);
                if (GUILayout.Button("Insatll URP Package"))
                {
                    UnityEditor.PackageManager.UI.Window.Open(URP_PACKAGE);
                }
            }
            m_pipelineAssetInstalled = GraphicsSettings.defaultRenderPipeline != null;
            m_forwardRendererData = EditorGUIHelper.GetDefaultRenderer() as UniversalRendererData;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Universal RP Package : ");
                if (!m_urpInstalled)
                {
                    EditorGUILayout.LabelField("Not install yet", errorStyle);
                }
                else
                {
                    EditorGUILayout.LabelField("Installed!", correctStyle);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(!m_urpInstalled);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Universal Render Pipeline Asset : ");
                if (!m_urpInstalled || !m_pipelineAssetInstalled)
                {
                    EditorGUILayout.LabelField("Not Set", errorStyle);
                }
                else
                {
                    EditorGUILayout.LabelField("Setted!", correctStyle);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (!m_pipelineAssetInstalled)
                EditorGUILayout.LabelField("Setup the pipeline asset by dragging the URP pipeline asset into \n`Scriptable Render Pipeline Settings` Field under Project Settings/Grahpics.", GUILayout.Height(60));
            if (!m_urpInstalled || !m_pipelineAssetInstalled)
            {
                if (GUILayout.Button("Create pipeline asset & Open project settings"))
                {
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath("Assets", typeof(UnityEngine.Object));
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);

                    if (EditorApplication.ExecuteMenuItem("Assets/Create/Rendering/Universal Render Pipeline/Pipeline Asset (Forward Renderer)"))
                    {
                        Debug.Log("Setup the pipeline asset by dragging the URP pipeline asset into \n`Scriptable Render Pipeline Settings` Field under Project Settings/Grahpics.");
                        SettingsService.OpenProjectSettings("Project/Graphics");
                    }
                }
            }
            //GraphicsSettings.defaultRenderPipeline
            m_forwardRendererData = EditorGUILayout.ObjectField("", m_forwardRendererData, typeof(UniversalRendererData), true) as UniversalRendererData;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            if (m_urpInstalled && m_pipelineAssetInstalled)
            {
                EditorGUILayout.LabelField("All set! Try re-import the shader if it shows pink color only", correctStyle);
            }
        }
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}
