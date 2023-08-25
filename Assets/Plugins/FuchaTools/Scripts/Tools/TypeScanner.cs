using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FuchaTools
{
    internal static class TypeScanner
    {
        private static readonly Dictionary<Type, List<TypeInfo>> Cash;
        private static readonly List<Type> AllTypes;

        static TypeScanner()
        {
            try
            {
                Cash = new Dictionary<Type, List<TypeInfo>>();
                AllTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).ToList();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void SearchTypes(Type type, string name, List<TypeInfo> result)
        {
            var cash = GetCash(type);
            name = name.ToLowerInvariant();
            result.Clear();
            result.AddRange(cash.Where(t => t.NameShortLowerInvariant.Contains(name)));
        }

        private static IEnumerable<TypeInfo> GetCash(Type baseType)
        {
            if (Cash.TryGetValue(baseType, out var result))
            {
                return result;
            }
            
            var derivedTypes = AllTypes.Where(baseType.IsAssignableFrom);
            result = new List<TypeInfo>();
            
            foreach (var type in derivedTypes)
            {
                TypeInfo info;
                info.Type = type;
                info.NameShort = type.Name;
                info.Namespace = type.Namespace;
                info.NameShortLowerInvariant = type.Name.ToLowerInvariant();
                result.Add(info);
            }
            
            Cash[baseType] = result;
            
            return result;
        }
    }
}
