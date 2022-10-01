using System;
using System.Linq;
using System.Collections.Generic;
using riles.Cecil;
using riles.Cecil.Cil;

namespace riles.Weaver {
    public static class CecilExtensions {
        public static bool Is(this TypeReference td, Type type) {
            if (type.IsGenericType) {
                return td.GetElementType().FullName == td.FullName;
            }
            return td.FullName == type.FullName;
        }

        public static bool Is<T>(this TypeReference td) {
            return Is(td, typeof(T));
        }

        public static bool HasCustomAttribute<T>(this ICustomAttributeProvider provider, out CustomAttribute attr) where T : Attribute {
            if (provider.HasCustomAttributes) {
                var result = provider.CustomAttributes.FirstOrDefault(x => x.AttributeType.Is<T>());
                if (result != null) {
                    attr = result;
                    return true;
                }
            }
            attr = null;
            return false;
        }

        public static bool HasCustomAttribute<T>(this ICustomAttributeProvider provider) where T : Attribute {
            // return HasCustomAttribute<T>(provider, out _);
            return provider.CustomAttributes.Any(x => x.AttributeType.Is<T>());
        }

        public static bool Implements<T>(this TypeDefinition td) {
            TypeDefinition def = td;

            while (def != null) {
                if (def.Interfaces.Any(x => x.InterfaceType.Is<T>())) return true;

                try {
                    def = def.BaseType?.Resolve();
                } catch (AssemblyResolutionException) {
                    break;
                }
            }

            return false;
        }

        public static bool Implements<T>(this TypeReference tr) {
            return tr.Resolve().Implements<T>();
        }

        public static bool Inherits<T>(this TypeReference td) => Inherits(td.Resolve(), typeof(T));

        public static bool Inherits(this TypeReference td, Type baseClass) => Inherits(td.Resolve(), baseClass);

        public static bool Inherits<T>(this TypeDefinition td) => Inherits(td, typeof(T));

        public static bool Inherits(this TypeDefinition td, Type baseClass) {
            if (!td.IsClass) return false;

            TypeReference parent = td.BaseType;
            if (parent == null) return false;
            if (parent.Is(baseClass)) return true;
            if (parent.CanBeResolved()) return Inherits(parent.Resolve(), baseClass);

            return false;
        }

        public static bool CanBeResolved(this TypeReference parent) {
            while (parent != null) {
                if (parent.Scope.Name == "Windows") return false;
                if (parent.Scope.Name == "mscorlib") return parent.Resolve() != null;

                try {
                    parent = parent.Resolve().BaseType;
                } catch {
                    return false;
                }
            }
            return true;
        }

        public static MethodReference MakeHostInstanceGeneric(this MethodReference self, GenericInstanceType type, ModuleDefinition module) {
            MethodReference refer = new MethodReference(self.Name, self.ReturnType, type) {
                CallingConvention = self.CallingConvention,
                HasThis = self.HasThis,
                ExplicitThis = self.ExplicitThis
            };

            foreach (var param in self.Parameters) {
                refer.Parameters.Add(new ParameterDefinition(param.ParameterType));
            }

            foreach (var genParam in self.GenericParameters) {
                refer.GenericParameters.Add(new GenericParameter(genParam.Name, refer));
            }

            return module.ImportReference(refer);
        }

        public static FieldReference SpecializeField(this FieldReference self, GenericInstanceType type) {
            return new FieldReference(self.Name, self.FieldType, type);
        }

        public static bool IsMultidimensionalArray(this TypeReference type) {
            return type is ArrayType arrayType && arrayType.Rank > 1;
        }

        public static IEnumerable<FieldDefinition> FindAllPublicFields(this TypeReference refer) {
            return FindAllPublicFields(refer.Resolve());
        }

        public static IEnumerable<FieldDefinition> FindAllPublicFields(this TypeDefinition type) {
            while (type != null) {
                foreach (var field in type.Fields) {
                    if (field.IsStatic || field.IsPrivate) continue;
                    if (field.IsNotSerialized) continue;
                    yield return field;
                }

                try {
                    type = type.BaseType?.Resolve();
                } catch (AssemblyResolutionException) {
                    break;
                }
            }
        }

        /// <summary>
        /// Prevents an error when resolving from the same module.
        /// </summary>
        public static TypeDefinition SafeResolve(this TypeReference tr, ModuleDefinition module) {
            TypeDefinition definition = null;
            bool end = false;

            foreach (var type in module.Types) {
                if (tr.FullName == type.FullName) {
                    definition = type;
                    end = true;
                } else {
                    // check nested types
                    if (type.HasNestedTypes) {
                        foreach (var nested in type.NestedTypes) {
                            if (tr.FullName == nested.FullName) {
                                definition = nested;
                                end = true;
                                break;
                            }
                        }
                    }
                }

                if (end) break;
            }

            if (definition == null) {
                return tr.Resolve();
            } else {
                return definition;
            }
        }

        public static MethodReference MakeGenericMethod(this MethodReference self, params TypeReference[] arguments) {
            if (self.GenericParameters.Count != arguments.Length)
                throw new ArgumentException();

            var instance = new GenericInstanceMethod(self);
            foreach (var argument in arguments)
                instance.GenericArguments.Add(argument);

            return instance;
        }

        // public static void RemoveAt(this ILProcessor processor, int index) {
        //     processor.Remove(processor.Body.Instructions[index]);
        // }
    }
}
