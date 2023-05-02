using System.Collections.Generic;
using UnityEditor;

namespace PFC.Toolkit.Core.Helpers {

    [InitializeOnLoad]
    public abstract class PFCPreferenceHelpers {
        public static Dictionary<string, PFCPreferenceHelpers> Preferences = new Dictionary<string, PFCPreferenceHelpers>();

        internal const string PREF_PREFIX = "PFCTOOLS2PREF";
        public string path = "pfc.unassigned";
        public string name = "pfc.unnamed";

        public PFCPreferenceHelpers(string SettingName, string SettingPath) {
            this.path = SettingPath;
            this.name = SettingName;
            Preferences.Add(SettingPath, this);
        }
    }
    public class BoolPreferenceHandler : PFCPreferenceHelpers {

        public bool cachedValue = false;
        public bool defaultValue = false;

        public BoolPreferenceHandler(string SettingName, string SettingPath, bool defaultValue = true) : base(SettingName, SettingPath) {
            this.defaultValue = defaultValue;
        }

        public bool IsEnabled {
            get { bool val = EditorPrefs.GetBool(PREF_PREFIX + path, defaultValue); cachedValue = val; return val; }
            set { EditorPrefs.SetBool(PREF_PREFIX + path, value); cachedValue = value; }
        }

        public void Toggle() {
            IsEnabled = !IsEnabled;
        }
    }
}