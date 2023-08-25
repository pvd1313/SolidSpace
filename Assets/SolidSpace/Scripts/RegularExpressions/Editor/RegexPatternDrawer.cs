#if ODIN_INSPECTOR

using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.RegularExpressions.Editor
{
    public class RegexPatternDrawer : OdinValueDrawer<RegexPattern>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var hasLabel = label != null;
            var rect = EditorGUILayout.GetControlRect(false, 20);
            var value = ValueEntry.SmartValue;
            var valueName = hasLabel ? label.text : ObjectNames.NicifyVariableName(nameof(value.pattern));
            value.pattern = EditorGUI.TextField(rect, valueName, value.pattern);
            
            ValueEntry.SmartValue = value;
        }
    }
}

#endif