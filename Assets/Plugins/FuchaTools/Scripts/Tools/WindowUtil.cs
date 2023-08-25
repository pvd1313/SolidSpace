using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FuchaTools
{
    internal static class WindowUtil
    {
        private const int WindowWidth = 200;
        private const int WindowHeight = 300;
        
        private static readonly HashSet<EditorWindow> NonInitializedWindows;

        static WindowUtil()
        {
            NonInitializedWindows = new HashSet<EditorWindow>();
        }
        
        public static void OpenToolWindow<T>() where T : EditorWindow
        {
            var window = ScriptableObject.CreateInstance<T>();
            NonInitializedWindows.Add(window);
            window.ShowPopup();
        }

        public static void InitializeWindowPosition(EditorWindow window)
        {
            const int widthHalf = WindowWidth / 2;
            const int heightHalf = WindowHeight / 2;
            
            var currentEvent = Event.current;
            if (currentEvent == null || currentEvent.type == EventType.Ignore)
            {
                return;
            }

            if (NonInitializedWindows.Remove(window))
            {
                var mPos = currentEvent.mousePosition;
                window.position = new Rect(mPos.x - widthHalf, mPos.y - heightHalf, WindowWidth, WindowHeight);
            }
        }
    }
}