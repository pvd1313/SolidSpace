using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FuchaTools
{
    internal class TypeScannerWindow
    {
        private const string FilterFocusControlName = "FilterFocusControlName";
        
        private readonly SelectableListView<TypeInfo> _listView;
        private readonly Type _targetType;
        private readonly List<TypeInfo> _filteredTypes;

        private bool _focusedOnce;
        private string _filter;

        public TypeScannerWindow(Type targetType)
        {
            _targetType = targetType;
            _filteredTypes = new List<TypeInfo>();
            _listView = new SelectableListView<TypeInfo>(t => t.NameShort);
            _filter = string.Empty;
        }

        public bool OnGUI(out TypeInfo selectedType)
        {
            selectedType = default;

            var searchTextStyle = new GUIStyle(GUIStylesConfig.SearchField);
            GUI.SetNextControlName(FilterFocusControlName);
            _filter = EditorGUILayout.TextField(_filter, searchTextStyle);

            TypeScanner.SearchTypes(_targetType, _filter, _filteredTypes);

            var selectedIndex = _listView.OnGUI(-1, _filteredTypes);
            if (selectedIndex >= 0)
            {
                selectedType = _filteredTypes[selectedIndex];
                return true;
            }

            if (!_focusedOnce)
            {
                _focusedOnce = true;
                GUI.FocusControl(FilterFocusControlName);
            }
            
            return false;
        }
    }
}