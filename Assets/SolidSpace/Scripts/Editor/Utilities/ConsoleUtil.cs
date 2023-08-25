using System;
using System.Reflection;
using UnityEngine;

namespace SolidSpace.Editor.Utilities
{
    public static class ConsoleUtil
    {
        private static readonly MethodInfo ClearConsoleMethod;
        private static readonly bool IsClearConsoleMethodFound;
        
        static ConsoleUtil()
        {
            try
            {
                var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                var type = assembly.GetType("UnityEditor.LogEntries");
                ClearConsoleMethod = type?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
                IsClearConsoleMethodFound = !(ClearConsoleMethod is null);
            }
            catch (Exception e)
            {
                IsClearConsoleMethodFound = false;
                
                Debug.LogException(e);
            }
        }

        public static void ClearLog()
        {
            if (!IsClearConsoleMethodFound)
            {
                Debug.LogError("Clear console method was not found");
                return;
            }

            ClearConsoleMethod.Invoke(null, null);
        }
    }
}