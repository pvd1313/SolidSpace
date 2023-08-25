using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FuchaTools
{
    internal class GUIStyleScannerWindow : EditorWindow
    {
        private static readonly List<GUIStyle> Buffer = new List<GUIStyle>();
        
        private readonly SelectableListView<GUIStyle> _listView;
        private readonly RepaintEventsView _repaintEvents;
        
        private List<GUIStyle> _knownStyles;
        private int _selectedIndex;

        public GUIStyleScannerWindow()
        {
            _listView = new SelectableListView<GUIStyle>(s => s.name, GUILayout.Height(300));
            _repaintEvents = new RepaintEventsView(this);
        }

        private void OnGUI()
        {
            _repaintEvents.OnGUI();
            
            if (_knownStyles == null)
            {
                StaticFieldValueScanner.Scan(typeof(EditorWindow).Assembly, Buffer);
                _knownStyles = Buffer.Where(s => !string.IsNullOrWhiteSpace(s.name)).ToList();
            }

            _selectedIndex = _listView.OnGUI(_selectedIndex, _knownStyles);

            EditorGUILayout.Space(10);

            if (_selectedIndex < 0)
            {
                return;
            }

            var selectedStyle = _knownStyles[_selectedIndex];
            GUILayout.TextField(selectedStyle.name, selectedStyle);
        }
    }
}