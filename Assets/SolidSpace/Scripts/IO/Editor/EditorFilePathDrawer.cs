#if ODIN_INSPECTOR

using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.IO.Editor
{
    public class EditorFilePathDrawer : OdinValueDrawer<EditorFilePath>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var hasLabel = label != null;
            var rect = EditorGUILayout.GetControlRect(false, 20);
            var value = ValueEntry.SmartValue;
            var valueName = hasLabel ? label.text : ObjectNames.NicifyVariableName(nameof(value.path));
            value.path = EditorGUI.TextField(rect, valueName, value.path);
            
            ValueEntry.SmartValue = value;
        }
    }
}

#endif