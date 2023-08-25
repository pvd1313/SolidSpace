#if ODIN_INSPECTOR

using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.Reflection.Editor
{
    public class TypeReferenceDrawer : OdinValueDrawer<TypeReference>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var hasLabel = label != null;
            var rect = EditorGUILayout.GetControlRect(false, 20);
            var value = ValueEntry.SmartValue;
            var valueName = hasLabel ? label.text : ObjectNames.NicifyVariableName(nameof(value.typeName));
            value.typeName = EditorGUI.TextField(rect, valueName, value.typeName);
            
            ValueEntry.SmartValue = value;
        }
    }
}

#endif