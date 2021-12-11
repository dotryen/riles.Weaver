using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace riles.Weaver {
    public class WeaverPreferences : EditorWindow {
        [MenuItem(MenuButtons.WEAVER_BUTTON_PREFIX + "Show Settings")]
        static void ShowWindow() {
            GetWindow<WeaverPreferences>(false, "riles.Weaver Preferences").Show();
        }

        private void OnGUI() {
            EditorGUILayout.LabelField("Ignored Assemblies", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("NOTE: Ignored assemblies are split with a semicolon (;)");
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("User ignored assemblies");
            EditorGUILayout.TextArea("");
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Script ignored assemblies");
            GUI.enabled = false;
            EditorGUILayout.TextArea("");
        }
    }
}
