using PFC.Toolkit.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PFC.Toolkit.Core.VersionManager {
#if PFC_DEBUG
    [CreateAssetMenu(fileName = "VersionManager", menuName = "PFC/DEBUG/VersionManager")]
#endif
    public class PFCAssetVersionManager : ScriptableObject {
        public string nextVersion = "1.0.0.0";
        public bool nextVersionIsBeta = false;
        public string _version = "1.0.0.0";
        public bool _versionIsBeta = false;
        public string _latestVersion = "1.0.0.0";
        public string owner = "";
        public string repo = "";
        public string[] FilePaths;
        public string _currentChangelog = "";
        public string newChangelog = "";
        public string packageUrl = "";

        [SerializeField]
        private string _tokenKey;
        public string TokenKey {
            get {
                return _tokenKey;
            }
            set {
                if (_tokenKey != value) {
                    _tokenKey = value;
                    Debug.Log($"Set: {_tokenKey} to {value}");
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        public void OnEnable() {

        }

        public Version GetVersion() { return new Version(_version); }
        public Version GetLatestVersion() { return new Version(_latestVersion); }

        public async Task PackageAndUpdateFiles() {
            try {
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();

                if (!AssetDatabase.IsValidFolder("Assets/GitHubExports")) {
                    AssetDatabase.CreateFolder("Assets", "GitHubExports");
                }

                string assetPath = $"Assets/GitHubExports/{name}_v{nextVersion}.unitypackage";
                List<string> files = new List<string>();
                foreach (string path in FilePaths) {
                    files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.AllDirectories));
                }

                string[] unprocessedFiles = files.ToArray();
                for (int i = 0; i < unprocessedFiles.Length; i++) {
                    string file = unprocessedFiles[i];
                    string extention = Path.GetExtension(file);
                    if (extention == ".git" || extention == ".gitignore" || extention == ".gitattributes") {
                        files.Remove(file);
                        Debug.Log("Removed:" + file);
                    }
                    float progress = (float)i / unprocessedFiles.Length;
                    EditorUtility.DisplayProgressBar("Processing files", file, progress);
                }

                EditorUtility.DisplayProgressBar("Processing files", $"Exporting: {assetPath}", 1);
                AssetDatabase.ExportPackage(files.ToArray(), assetPath, ExportPackageOptions.Recurse);
                AssetDatabase.Refresh();

                EditorUtility.DisplayProgressBar("Processing files", $"Uploading: {assetPath}", 1);
                string tag = nextVersion.ToString();
                if (_versionIsBeta) {
                    tag = "LatestBeta";
                    HttpResponseMessage deleteResponse = await PFCGitHubHelper.DeleteReleaseWithTag(owner, repo, PFCSettings.ActiveInstance.GetToken(this._tokenKey), tag);
                    Debug.Log($"Delete: {deleteResponse.StatusCode}\n{prettyPrint(await deleteResponse.Content.ReadAsStringAsync())}");
                }
                HttpResponseMessage response = await PFCGitHubHelper.CreateGitHubReleaseWithAsset(owner, repo, PFCSettings.ActiveInstance.GetToken(this._tokenKey), tag, _versionIsBeta, $"{name} v{nextVersion}", newChangelog, assetPath);
                Debug.Log($"Upload: {response.StatusCode}\n{prettyPrint(await response.Content.ReadAsStringAsync())}");
            }
            catch (Exception ex) {
                Debug.LogError(ex);
            }
            finally {
                EditorUtility.ClearProgressBar();
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        public async Task GetLatestPackageVersion() {

            HttpResponseMessage latestrelease = await PFCGitHubHelper.GetLatestGitHubRelease(owner, repo);
            string jsonString = await latestrelease.Content.ReadAsStringAsync();
            Debug.Log("GetUpdate:" + prettyPrint(jsonString));
            Match tagNameMatch = Regex.Match(jsonString, "\"tag_name\":\"([^\"]+)\"");
            Match bodyMatch = Regex.Match(jsonString, "\"body\":\"([^\"]+)\"");
            Match downloadUrlMatch = Regex.Match(jsonString, "\"browser_download_url\":\"([^\"]+)\"");

            if (tagNameMatch.Success) {
                _latestVersion = tagNameMatch.Groups[1].Value;
            }

            if (bodyMatch.Success) {
                _currentChangelog = bodyMatch.Groups[1].Value;
            }

            if (downloadUrlMatch.Success) {
                packageUrl = downloadUrlMatch.Groups[1].Value;
            }
        }

        public async Task DeletedReleaseByTag(string tag) {
            HttpResponseMessage deleted = await PFCGitHubHelper.DeleteReleaseWithTag(owner, repo, PFCSettings.ActiveInstance.GetToken(this._tokenKey), tag);
            string jsonString = await deleted.Content.ReadAsStringAsync();
            Debug.Log("DeleteVersion:" + prettyPrint(jsonString));
        }

        internal void AddLocalFilePath() {
            string path = AssetDatabase.GetAssetPath(this);
            path = Path.GetDirectoryName(path);
            List<string> files = new List<string>(FilePaths) {
                path
            };
            FilePaths = files.ToArray();
        }

        public static string prettyPrint(string input) {
            string output = "";
            int indent = 0;
            bool inString = false;
            foreach (char character in input) {

                if (character == '"') {
                    if (inString) {
                        output += character;
                    }

                    inString = !inString;
                }

                if (!inString) {
                    if (character == '{') {
                        indent++;
                        output += "{\n" + repeat("   ", indent);
                    }
                    else if (character == ',') {
                        output += ",\n" + repeat("   ", indent);
                    }
                    else if (character == '}') {
                        indent--;
                        output += "}\n" + repeat("   ", indent);
                    }
                    else if (character == ':') {
                        output += " : ";
                    }
                }
                else {
                    output += character;
                }
            }

            return output;
        }
        public static string repeat(string input, int count) {
            string output = "";
            for (int i = 0; i < count; i++) {
                output += input;
            }
            return output;
        }
    }
}