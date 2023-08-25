using System.IO;
using System.Linq;
using SolidSpace.IO.Editor;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace SolidSpace.Editor.Automation.Resume.FileCopyTool
{
    public class FileCopyToolAsset : ScriptableObject
    {
        [SerializeField] private FileCopyToolConfig _config;
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void PreviewDeletedFiles()
        {
            var outputFolder = EditorPath.Combine(EditorPath.ProjectRoot, _config.OutputFolder.path);
            var files = Directory.GetFiles(outputFolder);
            foreach (var file in files)
            {
                if (_config.DeleteFilesPatterns.Any(p => p.IsMatch(file)))
                {
                    Debug.Log(file);
                }
            }
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void PreviewExportedFiles()
        {
            foreach (var asset in _config.AssetsToCopy)
            {
                GetAssetCopyOperationPath(asset, out var fromPath, out var toPath);
                
                Debug.Log($"'{fromPath}' -> '{toPath}'");
            }
        }
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void DeleteOldFilesAndExportNewAssets()
        {
            var outputFolder = EditorPath.Combine(EditorPath.ProjectRoot, _config.OutputFolder.path);
            
            var files = Directory.GetFiles(outputFolder);
            foreach (var file in files)
            {   
                if (_config.DeleteFilesPatterns.Any(p => p.IsMatch(file)))
                {
                    File.Delete(file);
                }
            }

            foreach (var asset in _config.AssetsToCopy)
            {
                GetAssetCopyOperationPath(asset, out var fromPath, out var toPath);
                File.Copy(fromPath, toPath, false);
            }
            
            Debug.Log("Done");
        }

        private void GetAssetCopyOperationPath(Object asset, out string sourcePath, out string outputPath)
        {
            var outputFolder = EditorPath.Combine(EditorPath.ProjectRoot, _config.OutputFolder.path);
            var assetRelativePath = AssetDatabase.GetAssetPath(asset);
            var assetName = Path.GetFileName(assetRelativePath);
            sourcePath = EditorPath.Combine(EditorPath.ProjectRoot, assetRelativePath);
            outputPath = EditorPath.Combine(outputFolder, assetName);
        }
    }
}