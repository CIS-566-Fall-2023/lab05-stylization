// Creates a simple wizard that lets you create a Light GameObject
// or if the user clicks in "Apply", it will set the color of the currently
// object selected to red

using UnityEditor;
using UnityEngine;

public class LegacyGradientDataTransferWizard : ScriptableWizard
{
    private const string filePath = "Assets/OToon- URP Toon Shading/Data/GradientSettingsManager.asset";

    [MenuItem("Tools/OToon/Legacy Gradient Data Transfer Wizard")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<LegacyGradientDataTransferWizard>("OToon Update Wizard", "Auto Setup");
    }

    void OnWizardCreate()
    {
        var gsm = AssetDatabase.LoadAssetAtPath<GradientSettingsManager>(filePath);
        if (gsm != null)
        {
            TransferGradientData(gsm);
            AssetDatabase.DeleteAsset(filePath);
        }
    }

    void OnWizardUpdate()
    {
        helpString = "Legacy Asset : GradientSettingsManager detected! \n\nPrior to OToon ver1.1, ramp texture's gradient key are stored inside GradientSettingsManager.asset.\nWhich is no longer the case start from OToon 1.1. OToon now store those gradient key inside texture's meta file itself.\nThis make sure the gradient key data will never lost even something went wrong on the gradientSettingsMaanager.asset file.";
        helpString += "\n\nPlease Hit the *Auto Setup* button to transfer all gradient keys stored inside GradientSettingsManager.asset to it's correspond ramp texture's meta file.";
        helpString += "\nAfter transfered, check your gradient key setup via material editor. All gradient keys on material editor should be back as normal.";
        helpString += "\n\n\n TLDR : Please click the *Auto Setup* button when you see this window";
    }

    public bool TransferGradientData(GradientSettingsManager gsm)
    {
        var updateGraidentDataStatus = false;
        gsm.UpdateDataMap();

        foreach (var kvp in gsm.LatestGradientMap)
        {
            var texGUID = kvp.Key;
            var path = AssetDatabase.GUIDToAssetPath(texGUID);
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null)
            {
                var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                textureImporter.userData = JsonUtility.ToJson(kvp.Value);
                textureImporter.SaveAndReimport();
                updateGraidentDataStatus = true;
            }
        }
        if (updateGraidentDataStatus)
        {
            Debug.Log("Successful transfer legacy gradient settings data to texture's user data!");
        }
        return updateGraidentDataStatus;
    }
}