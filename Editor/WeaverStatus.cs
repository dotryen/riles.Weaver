using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace riles.Weaver {
    public static class WeaverStatus {
        #region Constants

        internal const string WEAVER_BUTTON_PREFIX = "rilesUtil/Weaver/";
        internal const string PAUSE_BUTTON = WEAVER_BUTTON_PREFIX + "Pause Weaver";
        internal const string FORCE_WEAVE = WEAVER_BUTTON_PREFIX + "Weave";
        internal const string STOP_LOG = WEAVER_BUTTON_PREFIX + "Hide Logs";

        internal const string WEAVER_READY = "BASIC_WEAVER_READY";
        internal const string WEAVER_PAUSE = "BASIC_WEAVER_PAUSE";
        internal const string WEAVER_TRY = "BASIC_WEAVER_TRY";
        internal const string WEAVER_FAILED = "BASIC_WEAVER_FAIL";
        internal const string WEAVER_STARTUP = "BASIC_WEAVER_START";
        internal const string COMP_FAILED = "BASIC_COMP_FAILED";

        #endregion

        /// <summary>
        /// Used to prevent reloading of values.
        /// </summary>
        public static bool Ready {
            get {
                return SessionState.GetBool(WEAVER_READY, false);
            }

            internal set {
                SessionState.SetBool(WEAVER_READY, value);
            }
        }

        public static bool Paused {
            get {
                return SessionState.GetBool(WEAVER_PAUSE, false);
            }

            internal set {
                SessionState.SetBool(WEAVER_PAUSE, value);
            }
        }

        public static bool TriedWeave {
            get {
                return SessionState.GetBool(WEAVER_TRY, false);
            }

            internal set {
                SessionState.SetBool(WEAVER_TRY, value);
            }
        }

        public static bool WeaveFailed {
            get {
                return SessionState.GetBool(WEAVER_FAILED, false);
            }

            internal set {
                SessionState.SetBool(WEAVER_FAILED, value);
            }
        }

        public static bool StartupWeave {
            get {
                return SessionState.GetBool(WEAVER_STARTUP, false);
            }
            internal set {
                SessionState.SetBool(WEAVER_STARTUP, value);
            }
        }

        public static bool CompilationFailed {
            get {
                return SessionState.GetBool(COMP_FAILED, false);
            }
            internal set {
                SessionState.SetBool(COMP_FAILED, value);
            }
        }

        internal static void Initialize() {
            if (Ready) return;

            Paused = false;
            TriedWeave = false;
            WeaveFailed = false;
            StartupWeave = false;
            CompilationFailed = false;
            Ready = true;
        }

        [MenuItem(PAUSE_BUTTON)]
        static void PauseButton() {
            if (!Paused) {
                if (!EditorUtility.DisplayDialog("Are you sure?", "Pausing will prevent you from entering Play Mode.", "Don't Pause", "Pause")) {
                    Paused = true;
                }
            } else {
                Paused = false;

                if (TriedWeave) {
                    WeaverHook.WeaveExistingAssemblies();
                    TriedWeave = false;
                }
            }
        }

        [MenuItem(PAUSE_BUTTON, true)]
        static bool PauseValidate() {
            Menu.SetChecked(PAUSE_BUTTON, Paused);
            return true;
        }

        // [MenuItem(STOP_LOG)]
        // static void StopLogButton() {
        // 
        // }

        [MenuItem(FORCE_WEAVE)]
        static void DoWeave() {
            WeaverHook.WeaveExistingAssemblies();
        }
    }
}
