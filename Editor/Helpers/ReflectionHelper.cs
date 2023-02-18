using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NewGraph {
    public static class ReflectionHelper {
        public static bool DoesTypeSupportInterface(Type type, Type inter) {
            if (inter.IsAssignableFrom(type))
                return true;
            if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == inter))
                return true;
            return false;
        }

        public static IEnumerable<Assembly> GetReferencingAssemblies(Assembly assembly) {
            return AppDomain
                .CurrentDomain
                .GetAssemblies().Where(asm => asm.GetReferencedAssemblies().Any(asmName => AssemblyName.ReferenceMatchesDefinition(asmName, assembly.GetName())));
        }

        public static IEnumerable<Type> TypesImplementingInterface(Type desiredType) {
            var assembliesToSearch = new Assembly[] { desiredType.Assembly }
                .Concat(GetReferencingAssemblies(desiredType.Assembly));
            return assembliesToSearch.SelectMany(assembly => assembly.GetTypes())
                .Where(type => DoesTypeSupportInterface(type, desiredType));
        }

        public static IEnumerable<Type> NonAbstractTypesImplementingInterface(Type desiredType) {
            return TypesImplementingInterface(desiredType).Where(t => !t.IsAbstract);
        }
    }
}
