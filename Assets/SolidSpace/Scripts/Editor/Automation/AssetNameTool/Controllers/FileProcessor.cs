using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.Editor.Automation.AssetNameTool
{
    internal class FileProcessor
    {
        public void Process(Config config, ICollection<FileInfo> result)
        {
            result.Clear();
            
            var assetGUIDs = AssetDatabase.FindAssets("t:ScriptableObject");
            var filters = config.Folders;
            
            for (var i = 0; i < assetGUIDs.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

                for (var j = 0; j < filters.Count; j++)
                {
                    var filter = filters[j];

                    if (!filter.enabled)
                    {
                        continue;
                    }

                    if (!filter.scannerFilter.IsMatch(assetPath))
                    {
                        continue;
                    }

                    var obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                    var typeName = obj.GetType().ToString();
                    var oldName = Path.GetFileName(assetPath);
                    var newName = filter.nameConverter.Replace(typeName);
                    var assetRoot = assetPath.Substring(0, assetPath.LastIndexOf(oldName));

                    result.Add( new FileInfo
                    {
                        foundByRegexId = j,
                        typeName = typeName,
                        originalPath = assetPath,
                        modifiedPath = assetRoot + newName
                    });
                }
            }
        }
    }
}