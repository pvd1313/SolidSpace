using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FuchaTools
{
    internal static class StaticFieldValueScanner
    {
        private static readonly Dictionary<Assembly, List<Type>> AssemblyTypesCash;

        static StaticFieldValueScanner()
        {
            AssemblyTypesCash = new Dictionary<Assembly, List<Type>>();
        }

        public static void Scan<T>(Assembly assembly, List<T> result) where T : class
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            
            var targetType = typeof(T);

            var query = GetAssemblyTypes(assembly)
                .Where(t => !t.IsGenericType)
                .SelectMany(a => a.GetFields(flags))
                .Where(f => f.FieldType == targetType)
                .Select(f => f.GetValue(null) as T)
                .Where(v => v != null);
            
            result.Clear();
            result.AddRange(query);
        }

        private static IEnumerable<Type> GetAssemblyTypes(Assembly assembly)
        {
            if (AssemblyTypesCash.TryGetValue(assembly, out var result))
            {
                return result;
            }

            result = assembly.GetTypes().ToList();
            AssemblyTypesCash[assembly] = result;

            return result;
        }
    }
}