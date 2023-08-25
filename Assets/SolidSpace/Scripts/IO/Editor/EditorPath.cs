using System;
using UnityEngine;

namespace SolidSpace.IO.Editor
{
    public static class EditorPath
    {
        public static string ProjectRoot { get; }
        
        static EditorPath()
        {
            try
            {
                ProjectRoot = Application.dataPath.Substring(0, Application.dataPath.Length - 7);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static string Combine(string pathA, string pathB)
        {
            return pathA + '/' + pathB;
        }
    }
}