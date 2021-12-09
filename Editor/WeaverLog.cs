using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal static class WeaverLog {
    public static void Log(object value) {
        Debug.Log($"Weaver Log: {value}");
    }

    public static void LogWarning(object value) {
        Debug.Log($"Weaver Warning: {value}");
    }

    public static void LogError(object value) {
        Debug.Log($"Weaver Error: {value}");
    }
}
