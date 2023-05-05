using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace PFC.Toolkit.Core {

#if PFC_DEBUG
    [CreateAssetMenu(fileName = "Settings", menuName = "PFC/DEBUG/Settings", order = 0)]
#endif
    public class PFCSettings : ScriptableObject, ISerializationCallbackReceiver {

        [JsonProperty("Tokens")]
        public Dictionary<string, string> Tokens = new Dictionary<string, string>();

        [JsonIgnore]
        [SerializeField]
        public string SerializedData = "";

        [JsonIgnore]
        [NonSerialized]
        private static List<PFCSettings> Settings = new List<PFCSettings>();
        public static ref List<PFCSettings> GetSettingFiles() {
            Settings.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(PFCSettings)}");
            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PFCSettings instance = AssetDatabase.LoadAssetAtPath<PFCSettings>(path);
                Settings.Add(instance);
            }
            return ref Settings;
        }

        public void OnBeforeSerialize() {
            SerializedData = JsonConvert.SerializeObject(Tokens);

        }
        public void OnAfterDeserialize() {
            JsonConvert.PopulateObject(SerializedData, Tokens);
        }
    }
}