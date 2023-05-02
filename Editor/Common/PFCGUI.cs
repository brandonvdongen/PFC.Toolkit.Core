using UnityEditor;
using UnityEngine;

namespace PFC.Core {
    public class PFCGUI {

        public static void HorizontalLine(Color? color = null, float height = 0.5f) {
            Color col = new Color(.5f, .5f, .5f, .5f);
            if (color != null) {
                col = (Color)color;
            }

            Rect rect = EditorGUILayout.GetControlRect(false, 0);
            rect.height = height;
            rect.width = rect.width + (rect.position.x * 2 + 1);
            rect.position = new Vector2(0, rect.position.y + 1);
            EditorGUI.DrawRect(rect, col);
        }
        public static void Spacer(float height) {
            Rect rect = EditorGUILayout.GetControlRect(false, 0);
            rect.height = height;
            rect.width = rect.width + (rect.position.x * 2 + 1);
            rect.position = new Vector2(0, rect.position.y + 1);
            EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0));
        }
    }
}