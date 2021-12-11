using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using UnityEngine;
using UnityEditor;
using UAssembly = UnityEditor.Compilation.Assembly;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace riles.Weaver {
    internal static class WeaverMaster {
        const string GENERATED_NAMESPACE = "riles.Generated";
        const string GENERATED_CLASS = "GenCode";

        public static TypeDefinition CurrentGeneratedClass {
            get {
                return genClass;
            }
        }

        static TypeDefinition genClass = null;
        static List<Weaver> editorWeavers = new List<Weaver>();
        static List<Weaver> playerWeavers = new List<Weaver>();

        internal static void Start() {
            foreach (var asm in AsmUtil.GetUserAssemblies(Platform.Editor)) {
                if (!File.Exists(asm.outputPath)) continue;

                GetDefinition(asm.outputPath, AsmUtil.GetDependencies(asm.outputPath).ToArray(), out var def, out var resolver);
                var weaverRefs = def.MainModule.Types.Where(x => x.Inherits<Weaver>());

                if (weaverRefs.Count() > 0) {
                    var sysAsm = asm.LoadAssembly();
                    var weavers = weaverRefs.Select(x => (Weaver)Activator.CreateInstance(sysAsm.GetType(x.FullName)));

                    foreach (var weav in weavers) {
                        if (weav.IsEditor) editorWeavers.Add(weav);
                        else playerWeavers.Add(weav);
                    }
                }

                def.Dispose();
                resolver.Dispose();
            }
        }

        internal static void Weave(string assembly, string[] depend, bool isEditor) {
            if (Path.GetFileNameWithoutExtension(assembly).StartsWith("Unity")) return;

            GetDefinition(assembly, depend, out var definition, out var resolver);
            if (HasGeneratedClass(definition.MainModule)) return;

            // work
            bool status = true;
            genClass = CreateGeneratedClass(definition.MainModule);

            foreach (var weaver in isEditor ? editorWeavers : playerWeavers) {
                weaver.Module = definition.MainModule;

                try {
                    weaver.Weave();
                } catch (Exception ex) {
                    Debug.LogError(ex);
                    status = false;
                    break;
                }
            }

            if (status) {
                definition.MainModule.Types.Add(genClass);
                definition.Write(new WriterParameters { WriteSymbols = true });
            }

            // dispose
            genClass = null;
            definition.Dispose();
            resolver.Dispose();

            WeaverStatus.WeaveFailed = !status;
        }

        internal static void End() {
            foreach (var edit in editorWeavers) {
                edit.Reset();
            }

            foreach (var play in playerWeavers) {
                play.Reset();
            }

            editorWeavers.Clear();
            playerWeavers.Clear();
        }

        private static TypeDefinition CreateGeneratedClass(ModuleDefinition module) {            
            return new TypeDefinition(GENERATED_NAMESPACE, GENERATED_CLASS,
                TypeAttributes.BeforeFieldInit | TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.Abstract | TypeAttributes.Sealed,
                module.ImportReference(typeof(object)));
        }

        private static bool HasGeneratedClass(ModuleDefinition module) {
            return module.GetTypes().Any(td => td.Namespace == GENERATED_NAMESPACE && td.Name == GENERATED_CLASS);
        }

        private static void GetDefinition(string assembly, string[] depend, out AssemblyDefinition definition, out DefaultAssemblyResolver resolver) {
            // create resolver
            resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(assembly));
            resolver.AddSearchDirectory(Path.GetDirectoryName(EditorApplication.applicationPath) + @"\Data\Managed");

            if (depend != null) {
                foreach (var str in depend) {
                    resolver.AddSearchDirectory(str);
                }
            }

            // get definition
            definition = AssemblyDefinition.ReadAssembly(assembly, new ReaderParameters { ReadWrite = true, ReadSymbols = true, AssemblyResolver = resolver });
        }
    }
}
