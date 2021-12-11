using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using Mono.Cecil;
using UnityEditor;
using UnityEditor.Compilation;
using UAssembly = UnityEditor.Compilation.Assembly;
using SysAssembly = System.Reflection.Assembly;

namespace riles.Weaver {
    internal static class AsmUtil {
        // GETS ASSEMBLIES FROM PIPELINE
        /// <summary>
        /// Tool to get Unity assemblies.
        /// </summary>
        /// <param name="platform">Which platform to search through.</param>
        /// <returns>Assemblies compiled by Unity.</returns>
        public static UAssembly[] GetAssemblies(Platform platform) {
            var assemblies = CompilationPipeline.GetAssemblies();
            return assemblies.Where(x => x.flags == (AssemblyFlags)platform && !x.name.StartsWith(AssemblyInfo.ASSEMBLY_NAME)).ToArray();
        }

        public static UAssembly GetAssemblyByName(string name) {
            var assemblies = CompilationPipeline.GetAssemblies();
            return assemblies.FirstOrDefault(x => x.name == name);
        }

        public static UAssembly GetAssemblyByPath(string path) {
            var assemblies = CompilationPipeline.GetAssemblies();
            return assemblies.FirstOrDefault(x => x.outputPath == path);
        }

        /// <summary>
        /// Gets assemblies that belong to the user. (Not Unity or Basically)
        /// </summary>
        /// <param name="platform">Which platform to search through.</param>
        /// <returns>All user assemblies.</returns>
        public static UAssembly[] GetUserAssemblies(Platform platform) {
            return GetUserAssemblies(GetAssemblies(platform));
        }

        public static UAssembly[] GetUserAssemblies(UAssembly[] assemblies) {
            return assemblies.Where(x => IsUserAssembly(x.name)).ToArray();
        }

        public static bool IsUserAssembly(string name) {
            return !name.StartsWith("Unity");
        }

        public static SysAssembly LoadAssembly(this UAssembly assembly) {
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.GetName().Name == assembly.name);
        }

        public static AssemblyDefinition LoadDefinition(this UAssembly assembly) {
            return AssemblyDefinition.ReadAssembly(assembly.outputPath);
        }

        public static HashSet<string> GetDependencies(string assemblyPath) {
            foreach (UAssembly asm in CompilationPipeline.GetAssemblies()) {
                if (asm.outputPath == assemblyPath) {
                    return GetDependencies(asm);
                }
            }
            return null;
        }

        public static HashSet<string> GetDependencies(UAssembly asm) {
            HashSet<string> depend = new HashSet<string> { Path.GetDirectoryName(asm.outputPath) };
            depend.Add(UnityEditorInternal.InternalEditorUtility.GetEngineCoreModuleAssemblyPath());

            foreach (string refer in asm.compiledAssemblyReferences) {
                depend.Add(Path.GetDirectoryName(refer));
            }

            return depend;
        }

        public static void ReloadAssemblies() {
#if UNITY_2019_3_OR_NEWER
            EditorUtility.RequestScriptReload();
#else
            UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
#endif
        }
    }
}
