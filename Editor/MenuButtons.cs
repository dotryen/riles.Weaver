using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace riles.Weaver {
    internal static class MenuButtons {
        internal const string MENU_PREFIX = "rilesUtil/";
        internal const string WEAVER_BUTTON_PREFIX = MENU_PREFIX + "Weaver/";
        internal const string PAUSE_BUTTON = WEAVER_BUTTON_PREFIX + "Pause Weaver";
        internal const string FORCE_WEAVE = WEAVER_BUTTON_PREFIX + "Weave";
        internal const string STOP_LOG = WEAVER_BUTTON_PREFIX + "Hide Logs";

        [MenuItem(MENU_PREFIX + "Recompile")]
        static void Recompile() {
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        }

        [MenuItem(MENU_PREFIX + "Reload")]
        static void RequestReload() {
            AsmUtil.ReloadAssemblies();
        }

        [MenuItem(PAUSE_BUTTON)]
        static void PauseButton() {
            if (!WeaverStatus.Paused) {
                if (!EditorUtility.DisplayDialog("Are you sure?", "Pausing will prevent you from entering Play Mode.", "Don't Pause", "Pause")) {
                    WeaverStatus.Paused = true;
                }
            } else {
                WeaverStatus.Paused = false;

                if (WeaverStatus.TriedWeave) {
                    WeaverHook.WeaveExistingAssemblies();
                    WeaverStatus.TriedWeave = false;
                }
            }
        }

        [MenuItem(PAUSE_BUTTON, true)]
        static bool PauseValidate() {
            Menu.SetChecked(PAUSE_BUTTON, WeaverStatus.Paused);
            return true;
        }

        [MenuItem(STOP_LOG)]
        static void StopLogButton() {
            WeaverStatus.LoggingPaused = !WeaverStatus.LoggingPaused;
        }

        [MenuItem(STOP_LOG, true)]
        static bool StopLogValidate() {
            Menu.SetChecked(STOP_LOG, WeaverStatus.LoggingPaused);
            return true;
        }

        [MenuItem(FORCE_WEAVE)]
        static void DoWeave() {
            WeaverHook.WeaveExistingAssemblies();
        }
    }
}
