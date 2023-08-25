using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.Editor.Automation.AssetNameTool
{
    internal class FileNameUtil
    {
        private readonly HashSet<string> _textHash;

        public FileNameUtil()
        {
            _textHash = new HashSet<string>();
        }
        
        public void Rename(IReadOnlyList<FileInfo> files)
        {
            _textHash.Clear();
            
            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                
                if (!CheckRequiresRenaming(file))
                {
                    _textHash.Add(file.modifiedPath.ToLowerInvariant());
                    continue;
                }
                
                var guid = AssetDatabase.AssetPathToGUID(file.modifiedPath);
                if (!string.IsNullOrEmpty(guid))
                {
                    Debug.LogError($"Abort. Name '{file.modifiedPath}' is occupied.");
                    return;
                }

                if (!_textHash.Add(file.modifiedPath.ToLowerInvariant()))
                {
                    Debug.LogError($"Abort. Name '{file.modifiedPath}' will be duplicated.");
                    return;
                }

                var message = AssetDatabase.ValidateMoveAsset(file.originalPath, file.modifiedPath);
                if (!string.IsNullOrEmpty(message))
                {
                    Debug.LogError($"Abort. ValidateMoveAsset: '{message}'");
                    return;
                }
            }

            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (!CheckRequiresRenaming(file))
                {
                    continue;
                }

                var fileName = Path.GetFileName(file.modifiedPath);
                var message = AssetDatabase.RenameAsset(file.originalPath, fileName);
                if (!string.IsNullOrEmpty(message))
                {
                    Debug.LogError($"Abort. RenameAsset: '{message}'");
                    return;
                }
                
                Debug.Log($"Renamed '{file.originalPath}' -> '{file.modifiedPath}'; extraInfo: '{message}'");
            }
        }

        private bool CheckRequiresRenaming(FileInfo fileInfo)
        {
            const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;

            return string.Compare(fileInfo.originalPath, fileInfo.modifiedPath, comparison) != 0;
        }
    }
}