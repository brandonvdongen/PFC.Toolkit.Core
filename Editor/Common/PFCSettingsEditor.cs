using UnityEditor;
using UnityEngine;
namespace PFC.Core {
    [CustomEditor(typeof(PFCSettings))]
    public class PFCSettingsEditor : Editor {
        private PFCSettings settings;
        private string keyField;
        private string valueField;

        private void OnEnable() {
            settings = target as PFCSettings;
        }

        public override void OnInspectorGUI() {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Key", GUILayout.Width(50));
            keyField = GUILayout.TextField(keyField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Token:", GUILayout.Width(50));
            valueField = GUILayout.TextField(valueField);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Add")) {
                settings.SetToken(keyField, valueField);
            }
            PFCGUI.HorizontalLine();
            PFCGUI.Spacer(1f);

            int i = 0;
            foreach (string key in settings.TokensKeys) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(key, GUILayout.Width(100));
                EditorGUILayout.TextField(settings.GetToken(key));
                if (GUILayout.Button("x", GUILayout.Width(50))) {
                    settings.RemoveToken(key);
                }
                EditorGUILayout.EndHorizontal();
                i++;
            }
        }
    }
}