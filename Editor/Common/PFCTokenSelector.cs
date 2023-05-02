using UnityEditor;
using UnityEngine;

namespace PFC.Core {
    public class PFCTokenSelector {

        private string _selectedKey;

        public bool Select(string key) {
            Debug.Log($"Called with {key}");
            if (PFCSettings.ActiveInstance.TokensKeys.Length == 0) {
                return false;
            }

            if (!string.IsNullOrEmpty(_selectedKey) && !string.IsNullOrEmpty(key)) {
                _selectedKey = key;
            }

            string[] names = new string[PFCSettings.ActiveInstance.TokensKeys.Length + 1];
            names[0] = string.IsNullOrEmpty(key) ? "Select Token" : key;
            int i = 1;
            foreach (string token in PFCSettings.ActiveInstance.TokensKeys) {
                names[i++] = token;
            }

            int _selectedIndex = EditorGUILayout.Popup(0, names, GUILayout.MaxWidth(100), GUILayout.ExpandWidth(false));
            if (_selectedIndex > 0) {
                _selectedKey = names[_selectedIndex];
                return true;

            }
            return false;
        }

        public string SelectedKey { get { return _selectedKey; } }
    }
}