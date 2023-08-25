using SolidSpace.IO.Editor;
using UnityEngine;

namespace SolidSpace.Editor.Automation.Resume.ProjectStructureTool
{
    internal class ProjectStructureToolAsset : ScriptableObject
    {
        [SerializeField] private Config _config;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void ScanAndLog()
        {
            var console = new UnityConsole(true);
            var writer = new FileWriter();
            writer.Write(_config, console);
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void ScanAndExport()
        {
            var exportPath = EditorPath.Combine(EditorPath.ProjectRoot, _config.ExportPath.path);
            using var console = new FileConsole(exportPath, true);
            var writer = new FileWriter();
            writer.Write(_config, console);
            
            Debug.Log("Done.");
        }
    }
}