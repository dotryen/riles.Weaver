using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace riles.Weaver {
    internal static class WeaverLog {
        public static void Log(object value) {
            if (WeaverStatus.LoggingPaused) return;
            Debug.Log($"Weaver Log: {value}");
        }

        public static void LogWarning(object value) {
            if (WeaverStatus.LoggingPaused) return;
            Debug.LogWarning($"Weaver Warning: {value}");
        }

        public static void LogError(object value) {
            if (WeaverStatus.LoggingPaused) return;
            Debug.LogError($"Weaver Error: {value}");
        }
    }
}
