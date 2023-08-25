#if ODIN_INSPECTOR

sing Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.RegularExpressions.Editor
{
    public class RegexPatternSubstitutionDrawer : OdinValueDrawer<RegexPatternSubstitution>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var hasLabel = label != null;
            var rect = EditorGUILayout.GetControlRect(false, 40);
            rect.height = 20;

            var value = ValueEntry.SmartValue;
            var valueName = hasLabel ? label.text : ObjectNames.NicifyVariableName(nameof(value.pattern));
            value.pattern = EditorGUI.TextField(rect, valueName, value.pattern);
            
            rect.y += 20;
            valueName = hasLabel ? " " : ObjectNames.NicifyVariableName(nameof(value.substitution));
            value.substitution = EditorGUI.TextField(rect, valueName, value.substitution);

            ValueEntry.SmartValue = value;
        }
    }
}

#endif