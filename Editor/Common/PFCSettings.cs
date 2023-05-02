using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace PFC.Core {

#if PFC_DEBUG
    [CreateAssetMenu(fileName = "Settings", menuName = "PFC/DEBUG/Settings", order = 0)]
#endif
    public class PFCSettings : ScriptableObject {
        private static PFCSettings _activeInstance;

        public static PFCSettings ActiveInstance {
            get {
                return _activeInstance;
            }
        }

        private void OnEnable() {
            if (_activeInstance == null) { _activeInstance = this; }
        }
        [SerializeField]
        public string[] TokensKeys;
        [SerializeField]
        public string[] TokensValues;

        public string GetToken(string key) {
            List<string> keys = new List<string>(TokensKeys);
            int i = keys.IndexOf(key);
            if (i != -1) {
                return TokensValues[i];
            }
            else {
                return string.Empty;
            }
        }

        public int GetTokenIndex(string key) {
            List<string> keys = new List<string>(TokensKeys);
            return keys.IndexOf(key);
        }

        public void SetToken(string key, string token) {
            if (key == string.Empty) {
                return;
            }

            List<string> keys = new List<string>(TokensKeys);
            List<string> values = new List<string>(TokensValues);

            int i = keys.IndexOf(key);
            Debug.Log($"found {key} at {i}");
            if (i == -1) {
                keys.Add(key);
                values.Add(token);
            }
            else {
                values[i] = token;
            }

            if (token == string.Empty) {
                keys.RemoveAt(i);
                values.RemoveAt(i);
            }

            TokensKeys = keys.ToArray();
            TokensValues = values.ToArray();
            EditorUtility.SetDirty(this);
        }

        public void RemoveToken(string key) {
            SetToken(key, string.Empty);
        }

        public static void DrawSettingSelector() {
            EditorGUILayout.BeginHorizontal();
            PFCSettings lastInstance = _activeInstance;
            _activeInstance = EditorGUILayout.ObjectField("Settings:", _activeInstance, typeof(PFCSettings), false) as PFCSettings;
            if (_activeInstance == null) {
                string[] guids = AssetDatabase.FindAssets($"t:{nameof(PFCSettings)}");
                if (guids.Length > 0) {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    _activeInstance = AssetDatabase.LoadAssetAtPath<PFCSettings>(path);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}