
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace PFC.Toolkit.Core.VersionManager {
    public class PFCKeyValueList : VisualElement {

        public Action OnChanged;

        VisualElement listBody;
        List<string> keys = new List<string>();
        List<string> values = new List<string>();

        private void DrawContent() {
            listBody.Clear();
            for (int i = 0; i < Math.Min(keys.Count, values.Count); i++) {
                int index = i;
                var urlField = new TextField() { value = keys[index] };
                urlField.style.height = 18;
                urlField.style.flexBasis = Length.Percent(70);
                urlField.isDelayed = true;
                urlField.RegisterValueChangedCallback(ev => {
                    keys[index] = ev.newValue;
                    OnChanged?.Invoke();
                });
                listBody.Add(urlField);

                var versionField = new TextField() { value = values[index] };
                versionField.style.flexGrow = 1;
                versionField.style.height = 18;
                versionField.isDelayed = true;
                versionField.RegisterValueChangedCallback(ev => {
                    values[index] = ev.newValue;
                    OnChanged?.Invoke();
                });
                listBody.Add(versionField);

                var btn_delete = new Button() { text = $"X" };
                btn_delete.style.alignSelf = Align.FlexEnd;
                btn_delete.clicked += () => {
                    keys.RemoveAt(index);
                    values.RemoveAt(index);
                    listBody.Remove(btn_delete);
                    listBody.Remove(urlField);
                    listBody.Remove(versionField);
                    OnChanged?.Invoke();
                };
                listBody.Add(btn_delete);
            }
        }

        public void SetContent(Dictionary<string, string> content) {
            keys.Clear();
            values.Clear();

            foreach (KeyValuePair<string, string> kv in content) {
                keys.Add(kv.Key);
                values.Add(kv.Value);
            }
            DrawContent();
        }
        public void GetContent(ref Dictionary<string, string> dict) {
            dict.Clear();
            for (int i = 0; i < Math.Min(keys.Count, values.Count); i++) {
                dict.Add(keys[i], values[i]);
            }
        }
        public PFCKeyValueList() {

            var btn_add = new Button() { text = "add" };
            btn_add.clicked += () => {
                keys.Add("");
                values.Add("");
                DrawContent();
            };
            this.Add(btn_add);

            listBody = new VisualElement();
            listBody.style.flexDirection = FlexDirection.Row;
            listBody.style.flexWrap = Wrap.Wrap;
            listBody.style.flexShrink = 1;
            this.Add(listBody);
        }
    }
}