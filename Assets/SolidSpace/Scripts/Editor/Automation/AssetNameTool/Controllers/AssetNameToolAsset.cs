using System.Collections.Generic;
using SolidSpace.Editor.Utilities;
using UnityEngine;

namespace SolidSpace.Editor.Automation.AssetNameTool
{
    public class AssetNameToolAsset : ScriptableObject
    {
        [SerializeField] private Config _config;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif

        private void ScanAndLog()
        {
            ConsoleUtil.ClearLog();
            
            var processor = new FileProcessor();
            var files = new List<FileInfo>();
            processor.Process(_config, files);
            
            foreach (var file in files)
            {
                Debug.Log($"({file.typeName}) ({file.foundByRegexId}) '{file.originalPath}' -> '{file.modifiedPath}'");
            }
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void RenameAssets()
        {
            ConsoleUtil.ClearLog();
            
            var processor = new FileProcessor();
            var renamer = new FileNameUtil();
            var files = new List<FileInfo>();
            
            processor.Process(_config, files);
            renamer.Rename(files);
        }
    }
}