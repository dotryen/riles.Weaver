using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace riles.Weaver {
    internal static class EditorChecks {
        [InitializeOnLoadMethod]
        static void OnLoad() {
            EditorApplication.playModeStateChanged += PlayModeCheck;
        }

        static void PlayModeCheck(PlayModeStateChange change) {
            if (change == PlayModeStateChange.ExitingEditMode) {
                if (WeaverStatus.WeaveFailed || WeaverStatus.TriedWeave) {
                    WeaverHook.WeaveExistingAssemblies();

                    if (WeaverStatus.WeaveFailed) {
                        EditorApplication.isPlaying = false;

                        WeaverLog.LogError("Attempts at weaving have failed. Cannot enter play mode until issues are resolved.");

                        foreach (SceneView view in SceneView.sceneViews) {
                            view.ShowNotification(new GUIContent("All weaver errors must be fixed before playing!"));
                        }
                    }
                }
            }

#if UNITY_2019_3_OR_NEWER
            if (EditorSettings.enterPlayModeOptionsEnabled) {
                WeaverLog.LogError("Enter Play mode options are not supported.");
                EditorApplication.isPlaying = false;
            }
#endif
        }
    }
}

