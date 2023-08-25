using System.Diagnostics;
using UnityEngine;

namespace SolidSpace.Editor.Automation.Resume.CommandLineTool
{
    public class CommandLineToolAsset : ScriptableObject
    {
        [SerializeField] private CommandLineToolConfig _config;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void InvokeCommandLines()
        {
            foreach (var command in _config.CommandLines)
            {
                Process.Start("cmd.exe",  $"/c \"{command}\"");
            }
        }
    }
}