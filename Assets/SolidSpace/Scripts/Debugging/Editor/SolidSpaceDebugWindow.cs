using UnityEditor;

namespace SolidSpace.Debugging.Editor
{
    internal class SolidSpaceDebugWindow : EditorWindow
    {
        private void OnGUI()
        {
            foreach (var floatState in SpaceDebug.FloatStates)
            {
                EditorGUILayout.FloatField(floatState.Key, floatState.Value);
            }

            foreach (var intState in SpaceDebug.IntStates)
            {
                EditorGUILayout.IntField(intState.Key, intState.Value);
            }

            Repaint();
        }
    }
}