using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace riles.Weaver {
    public static class WeaverStatus {
        #region Constants

        internal const string WEAVER_READY = "WEAVER_READY";
        internal const string WEAVER_PAUSE = "WEAVER_PAUSE";
        internal const string WEAVER_TRY = "WEAVER_TRY";
        internal const string WEAVER_FAILED = "WEAVER_FAIL";
        internal const string WEAVER_STARTUP = "WEAVER_START";
        internal const string COMP_FAILED = "WEAVER_COMP_FAILED";
        internal const string LOG_PAUSED = "WEAVER_LOG_PAUSED";

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

        public static bool LoggingPaused {
            get {
                return EditorPrefs.GetBool(LOG_PAUSED, false);
            }
            internal set {
                EditorPrefs.SetBool(LOG_PAUSED, value);
            }
        }

        internal static void Initialize() {
            if (Ready) return;

            Paused = false;
            TriedWeave = false;
            WeaveFailed = false;
            StartupWeave = false;
            CompilationFailed = false;
            // LoggingPaused = false;

            Ready = true;
        }
    }
}
