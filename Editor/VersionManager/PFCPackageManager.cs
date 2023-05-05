using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PFC.Toolkit.Core.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "PFC/VRCVersionControl", fileName = "PackageManager")]
public class PFCPackageManager : ScriptableObject {
    public TextAsset PackageFile;
    public PFCPackageInfo PackageInfo;

    public string ExportName => $"{PackageInfo.DisplayName}-{PackageInfo.Version}";

    public static List<PFCPackageManager> PackageList = new List<PFCPackageManager>();

    private void OnEnable() {
        Load();
    }

    public static ref List<PFCPackageManager> GetPackages() {
        Debug.Log("Discovering Packages");
        PackageList.Clear();
        string[] guids = AssetDatabase.FindAssets($"t:{nameof(PFCPackageManager)}");
        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PFCPackageManager instance = AssetDatabase.LoadAssetAtPath<PFCPackageManager>(path);
            instance.Load();
            PackageList.Add(instance);
        }
        return ref PackageList;
    }
    public override string ToString() {
        if (!string.IsNullOrEmpty(PackageInfo.DisplayName)) return PackageInfo.DisplayName;
        if (!string.IsNullOrEmpty(PackageInfo.Name)) return PackageInfo.Name;
        return AssetDatabase.GetAssetPath(this);
    }
    public void Load() {
        if (PackageFile == null) {
            return;
        }

        PackageInfo = JsonConvert.DeserializeObject<PFCPackageInfo>(PackageFile.text);
        PackageInfo.LocalPath = AssetDatabase.GetAssetPath(this);
    }

    public void Save() {
        string path = AssetDatabase.GetAssetPath(PackageFile);
        string packageInfo = JsonConvert.SerializeObject(PackageInfo, Formatting.Indented);

        JObject packageJson = JObject.Parse(packageInfo);
        JObject original = JObject.Parse(PackageFile.text);


        foreach (var kvp in original) {
            var key = kvp.Key;
            var value = kvp.Value;
            if (!packageJson.ContainsKey(key)) {
                packageJson.Add(key, value);
            }
        }

        File.WriteAllText(path, packageJson.ToString());
        AssetDatabase.Refresh();
    }

    public void ExportPackage() {
        AssetDatabase.SaveAssets();
        Save();
        if (!AssetDatabase.IsValidFolder("Assets/Exports")) {
            AssetDatabase.CreateFolder("Assets", "Exports");
        }
        var path = $"Assets/Exports/{ExportName}.unitypackage";
        AssetDatabase.ExportPackage(AssetDatabase.GetAssetPath(this), path);
        AssetDatabase.Refresh();
        PackageInfo.ZipSHA256 = PFCHashHelper.CalculateSHA256(path);
        Save();
    }
}
