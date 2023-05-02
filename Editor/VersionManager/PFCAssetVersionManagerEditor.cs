using System;
using UnityEditor;
using UnityEngine;

namespace PFC.Core.VersionManager {
    [CustomEditor(typeof(PFCAssetVersionManager))]
    public class PFCAssetVersionManagerEditor : Editor {
        private bool ShowSettings = false;
        private bool ShowToken = false;
        private PFCTokenSelector tokenSelector;

        private void OnEnable() {
            tokenSelector = new PFCTokenSelector();
        }

        public override void OnInspectorGUI() {
            PFCAssetVersionManager versionManager = (PFCAssetVersionManager)target;
            if (GUILayout.Button("Check Version")) {
                _ = versionManager.GetLatestPackageVersion();
            }
            GUI.enabled = false;
            GUILayout.Label("Version:");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Current", versionManager._version);
            if (versionManager._versionIsBeta) {
                GUILayout.Label("Beta");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.TextField("Latest", versionManager._latestVersion);
            GUILayout.Label("Info:");
            EditorGUILayout.TextField("UnityPackage URL", versionManager.packageUrl);
            GUILayout.Label("Changelog:");
            EditorGUILayout.TextArea(versionManager._currentChangelog);

            GUI.enabled = true;
            ShowSettings = EditorGUILayout.Foldout(ShowSettings, "Settings");
            if (ShowSettings) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                GUILayout.Label("Version");
                GUILayout.Label("Beta");
                GUILayout.Label("Owner");
                GUILayout.Label("Repo");
                GUILayout.Label("Token");
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                versionManager.nextVersion = EditorGUILayout.TextField(versionManager.nextVersion);
                if (Version.TryParse(versionManager.nextVersion, out Version parsedVersion)) {
                    if (versionManager.nextVersion == versionManager._latestVersion) {
                        EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), new Color(1, 1, 0, .1f));
                    }

                    if (GUILayout.Button("Set")) {
                        versionManager._version = parsedVersion.ToString();
                        versionManager._versionIsBeta = versionManager.nextVersionIsBeta;
                        EditorUtility.SetDirty(versionManager);
                    }
                }
                else {
                    EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), new Color(1, 0, 0, .1f));
                    GUI.enabled = false;
                    GUILayout.Button("Set");
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
                versionManager.nextVersionIsBeta = EditorGUILayout.Toggle(versionManager.nextVersionIsBeta);
                versionManager.owner = EditorGUILayout.TextField(versionManager.owner);
                versionManager.repo = EditorGUILayout.TextField(versionManager.repo);
                EditorGUILayout.BeginHorizontal();
                if (tokenSelector.Select(versionManager.TokenKey)) {
                    versionManager.TokenKey = tokenSelector.SelectedKey;
                }
                if (ShowToken) {
                    EditorGUILayout.TextField(PFCSettings.ActiveInstance.GetToken(versionManager.TokenKey));
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_animationvisibilitytoggleon"), GUILayout.Width(30))) {
                        ShowToken = false;
                    }
                }
                if (!ShowToken) {
                    EditorGUILayout.PasswordField(PFCSettings.ActiveInstance.GetToken(versionManager.TokenKey));
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_animationvisibilitytoggleoff"), GUILayout.Width(30))) {
                        ShowToken = true;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                GUILayout.Label("");
                GUILayout.Label("Changelog");
                versionManager.newChangelog = EditorGUILayout.TextArea(versionManager.newChangelog);
                GUILayout.Label("");
                SerializedObject SO = new SerializedObject(versionManager);
                SerializedProperty prop = SO.FindProperty(nameof(PFCAssetVersionManager.FilePaths));
                EditorGUILayout.PropertyField(prop);
                SO.ApplyModifiedProperties();
                if (versionManager.owner.Length > 0 && versionManager.repo.Length > 0 && PFCSettings.ActiveInstance.GetToken(versionManager.TokenKey).Length > 0) {
                    if (GUILayout.Button("Upload")) {
                        _ = versionManager.PackageAndUpdateFiles();
                    }
                    if (GUILayout.Button("Add Current Folder")) {
                        versionManager.AddLocalFilePath();
                    }
                }
            }
        }
    }
}