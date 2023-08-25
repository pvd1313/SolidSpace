using UnityEditor;
using UnityEngine;

namespace SolidSpace.UI.Core.Editor
{
    [CustomPropertyDrawer(typeof(UIPrefab<>))]
    public class UIPrefabDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("_asset"), label);
        }
    }
}