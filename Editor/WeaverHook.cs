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
            WeaverStatus.CompilationFailed = WeaverStatus.CompilationFailed && messages.Any(x => x.type == CompilerMessageType.Error);
        }

        private static void OnCompileFinish(object obj) {
            if (WeaverStatus.CompilationFailed) return;

            if (WeaverStatus.Paused) {
                WeaverStatus.TriedWeave = true;
                return;
            }

            WeaverMaster.Start();

            foreach (var assem in CompilationPipeline.GetAssemblies()) {
                if (WeaverStatus.WeaveFailed) break;
                if (assem.name == Globals.ASSEMBLY_NAME) continue; // Do not weave weaver assembly

                WeaveAssembly(assem.outputPath);
            }

            WeaverMaster.End();
            ReloadAssemblies();
            if (!WeaverStatus.WeaveFailed) WeaverLog.Log("Successfully weaved through assemblies!");
            else WeaverLog.LogWarning("Weave failed.");
        }

        public static void WeaveExistingAssemblies() {
            if (WeaverStatus.CompilationFailed) {
                WeaverLog.Log("Damn bro, fix those errors first.");
            }

            OnCompileStart(null);
            WeaverMaster.Start();

            foreach (var assem in CompilationPipeline.GetAssemblies()) {
                if (WeaverStatus.WeaveFailed) {
                    break;
                    // add log
                }
                if (assem.name == Globals.ASSEMBLY_NAME) continue; // Do not weave weaver assembly

                WeaveAssembly(assem.outputPath);
            }

            WeaverMaster.End();
            ReloadAssemblies();
        }

        internal static void WeaveAssembly(string assemblyPath) {
            if (!File.Exists(assemblyPath)) return;

            var name = Path.GetFileNameWithoutExtension(assemblyPath);
            if (!AsmUtil.IsUserAssembly(name)) return;

            bool isEditor = assemblyPath.Contains(".Editor") || assemblyPath.Contains("-Editor");
            HashSet<string> depend = AsmUtil.GetDependencies(assemblyPath);

            WeaverStatus.WeaveFailed = !WeaverMaster.Weave(assemblyPath, depend.ToArray(), isEditor);
        }

        internal static void ReloadAssemblies() {
#if UNITY_2019_3_OR_NEWER
            EditorUtility.RequestScriptReload();
#else
            UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
#endif
        }
    }
}
