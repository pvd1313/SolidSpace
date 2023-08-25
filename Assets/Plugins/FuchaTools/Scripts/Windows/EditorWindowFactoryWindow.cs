using System.Text.RegularExpressions;
using UnityEditor;

namespace FuchaTools
{
    internal class EditorWindowFactoryWindow : EditorWindow
    {
        private const string NameRegex = "(Window$)|([A-Z][a-z0-9]+)";

        private readonly TypeScannerWindow _scannerWindow;
        private readonly RepaintEventsView _repaintEvents;
        
        [MenuItem("Window/Open...", priority = int.MinValue)]
        private static void OpenWindow()
        {
            WindowUtil.OpenToolWindow<EditorWindowFactoryWindow>();
        }

        public EditorWindowFactoryWindow()
        {
            _scannerWindow = new TypeScannerWindow(typeof(EditorWindow));
            _repaintEvents = new RepaintEventsView(this);
        }

        private void OnGUI()
        {
            _repaintEvents.OnGUI();
            
            WindowUtil.InitializeWindowPosition(this);

            if (!_scannerWindow.OnGUI(out var selectedType))
            {
                return;
            }

            var windowTitle = Regex.Replace(selectedType.NameShort, NameRegex, "$2 ").Trim();
            
            GetWindow(selectedType.Type, false, windowTitle);
            
            Close();
        }

        private void OnLostFocus()
        {
            Close();
        }
    }
}