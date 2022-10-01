using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace riles.Weaver {
    internal static class WeaverHook {
        [InitializeOnLoadMethod]
        static void OnLoad() {
            WeaverStatus.Initialize();

            CompilationPipeline.compilationStarted += OnCompileStart;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompileFinish;
            CompilationPipeline.compilationFinished += OnCompileFinish;

            if (!WeaverStatus.StartupWeave) {
                WeaveExistingAssemblies();
                WeaverStatus.StartupWeave = true;
                WeaverLog.Log("Weaver successfully loaded");
            }
        }

        private static void OnCompileStart(object obj) {
            WeaverStatus.CompilationFailed = false;
            WeaverStatus.WeaveFailed = false;
            WeaverStatus.TriedWeave = false;
        }

        private static void OnAssemblyCompileFinish(string assembly, CompilerMessage[] messages) {
            WeaverStatus.CompilationFailed = WeaverStatus.CompilationFailed || messages.Any(x => x.type == CompilerMessageType.Error);
        }

        private static void OnCompileFinish(object obj) {
            if (WeaverStatus.CompilationFailed) return;
            if (WeaverStatus.Paused) {
                WeaverStatus.TriedWeave = true;
                return;
            }

            WeaveAll();
        }

        public static void WeaveExistingAssemblies() {
            if (WeaverStatus.CompilationFailed) {
                WeaverLog.Log("Damn bro, fix those errors first.");
            }

            OnCompileStart(null);
            WeaveAll();
        }

        internal static void WeaveAll() {
            WeaverMaster.Start();

            {
                Assembly[] assemblies = AsmUtil.GetUserAssemblies(Platform.Player);

                for (int i = 0; i < assemblies.Length; i++) {
                    if (WeaverStatus.WeaveFailed) break;
                    EditorUtility.DisplayProgressBar("Weaver Status (Player pass)", $"Weaving {assemblies[i].name}...", (float)i / assemblies.Length);
                    WeaveAssembly(assemblies[i].outputPath, false);
                }
            }

            {
                Assembly[] assemblies = AsmUtil.GetUserAssemblies(Platform.Editor);

                for (int i = 0; i < assemblies.Length; i++) {
                    if (WeaverStatus.WeaveFailed) break;
                    EditorUtility.DisplayProgressBar("Weaver Status (Editor pass)", $"Weaving {assemblies[i].name}...", (float)i / assemblies.Length);
                    WeaveAssembly(assemblies[i].outputPath, true);
                }
            }

            WeaverMaster.End();
            EditorUtility.ClearProgressBar();

            AsmUtil.ReloadAssemblies();

            if (!WeaverStatus.WeaveFailed) WeaverLog.Log("Successfully weaved through assemblies!");
            else WeaverLog.LogWarning("Weave failed.");
        }

        internal static void WeaveAssembly(string assemblyPath, bool isEditor) {
            if (!File.Exists(assemblyPath)) return;

            var name = Path.GetFileNameWithoutExtension(assemblyPath);
            if (!AsmUtil.IsUserAssembly(name)) return;

            HashSet<string> depend = AsmUtil.GetDependencies(assemblyPath);
            WeaverMaster.Weave(assemblyPath, depend.ToArray(), isEditor);
        }
    }
}
