using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace FuchaTools
{
    [InitializeOnLoad]
    public static class FancyProjectBrowserTitle
    {
        private const int MaxTitleLength = 18;

        private static readonly Type ProjectBrowserType;
        private static readonly MethodInfo ProjectBrowser_GetActiveFolderPath;
        private static double LastUpdateTime;
        private static Texture Icon;
        private static Dictionary<string, string> PathToTitleCash;
        private static Dictionary<Object, string> WindowLastPath;
        private static HashSet<Object> KnownWindows;
        private static StringBuilder StringBuilder;

        static FancyProjectBrowserTitle()
        {
            try
            {
                var editorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
                ProjectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");
                if (ProjectBrowserType is null)
                {
                    Debug.LogError("Failed to resolve 'UnityEditor.ProjectBrowser'");
                    return;
                }

                if (!GetMethodInfo("GetActiveFolderPath", out ProjectBrowser_GetActiveFolderPath))
                {
                    return;
                }

                WindowLastPath = new Dictionary<Object, string>();
                PathToTitleCash = new Dictionary<string, string>();
                KnownWindows = new HashSet<Object>();
                StringBuilder = new StringBuilder();
                Icon = EditorGUIUtility.FindTexture("Project");

                EditorApplication.update -= OnApplicationUpdate;
                EditorApplication.update += OnApplicationUpdate;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static bool GetMethodInfo(string methodName, out MethodInfo methodInfo)
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
            
            methodInfo = ProjectBrowserType.GetMethod(methodName, flags);
            if (methodInfo is null)
            {
                Debug.LogError($"Failed to get method info 'ProjectBrowser.{methodName}()");
                return false;
            }

            return true;
        }

        private static void OnApplicationUpdate()
        {
            try
            {
                var time = EditorApplication.timeSinceStartup;
                if (LastUpdateTime + 1 > EditorApplication.timeSinceStartup)
                {
                    return;
                }

                LastUpdateTime = time;
                
                var allWindows = Resources.FindObjectsOfTypeAll(ProjectBrowserType);
                foreach (var windowObject in allWindows)
                {
                    // Window may not be initialized yet. This hack fixes it.
                    if (KnownWindows.Add(windowObject))
                    {
                        continue;
                    }
                    
                    var path = (string) ProjectBrowser_GetActiveFolderPath.Invoke(windowObject, null);

                    if (WindowLastPath.TryGetValue(windowObject, out var lastPath) && (lastPath == path))
                    {
                        continue;
                    }

                    WindowLastPath[windowObject] = path;
                    var title = PathToTitle(path);

                    var windowEditor = (EditorWindow) windowObject;
                    windowEditor.titleContent = new GUIContent(title, Icon, path);
                    windowEditor.Repaint();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                EditorApplication.update -= OnApplicationUpdate;
            }
        }

        private static string PathToTitle(string path)
        {
            if (PathToTitleCash.TryGetValue(path, out var title))
            {
                return title;
            }
            
            var folders = path.Split('/');

            StringBuilder.Clear();
            
            var counter = 0;
            var i = folders.Length - 1;
            
            for (; i >= 0 && (counter < MaxTitleLength); i--)
            {
                StringBuilder.Insert(0, folders[i]);
                if (i > 0)
                {
                    StringBuilder.Insert(0, '/');
                    counter++;
                }
                
                counter += folders[i].Length;
            }

            if (i > -1)
            {
                StringBuilder.Insert(0, "..");
            }

            title = StringBuilder.ToString();
            PathToTitleCash[path] = title;

            return title;
        }
    }
}