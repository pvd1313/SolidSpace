using System.Collections.Generic;
using System.IO;
using System.Linq;
using SolidSpace.Editor.Utilities;
using SolidSpace.IO.Editor;
using UnityEngine;

namespace SolidSpace.Editor.Automation.NamespaceTool
{
    internal class NamespaceToolAsset : ScriptableObject
    {
        [SerializeField] private Config _config;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void ScanFoldersAndLog()
        {
            ConsoleUtil.ClearLog();
            
            var folderScanner = new FolderScanner();
            var folders = folderScanner.Scan(_config);

            foreach (var info in folders)
            {
                var regex = _config.FolderFilters[info.regexId].filter.pattern;
                var message = $"'{info.name}' by regex '{regex}' ({info.regexId})";
                Debug.Log(message);
            }
            
            Debug.Log($"Total folders: {folders.Count};");
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void ScanAssembliesAndLog()
        {
            ConsoleUtil.ClearLog();

            var assemblyUtil = new AssemblyUtil();
            var assemblies = assemblyUtil.Scan(_config);

            foreach (var assembly in assemblies)
            {
                var regex = _config.AssemblyFilters[assembly.regexId].filter.pattern;
                var fileName = assemblyUtil.AssemblyToFileName(assembly.name);
                var message = $"'{assembly.name}' -> '{fileName}' by regex '{regex}' ({assembly.regexId}):";
                
                Debug.Log(message);
                
                foreach (var folder in assembly.folders)
                {
                    Debug.Log($"\t{folder}");
                }
            }

            var folderCount = assemblies.SelectMany(a => a.folders).Count();
            Debug.Log($"Total assemblies: {assemblies.Count}; Total folders: {folderCount}");
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void OverrideProjDotSettings()
        {
            var oldFiles = Directory.GetFiles(EditorPath.ProjectRoot, "*.csproj.DotSettings");
            foreach (var file in oldFiles)
            {
                File.Delete(file);
            }
            
            var folderScanner = new FolderScanner();
            var assemblyUtil = new AssemblyUtil();
            var dotSettingsWriter = new DotSettingsWriter();
            var folders = folderScanner.Scan(_config);
            var folderNames = new HashSet<string>(folders.Select(f => f.name));
            var assemblies = assemblyUtil.Scan(_config);
            foreach (var assembly in assemblies)
            {
                var assemblyPath = assemblyUtil.AssemblyToFileName(assembly.name);
                var foldersToSkip = assembly.folders.Where(f => folderNames.Contains(f));
                dotSettingsWriter.Write(assemblyPath, foldersToSkip);
            }

            Debug.Log("Done. Don't forget to restart Rider.");
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void RegexHelp()
        {
            Application.OpenURL("https://regex101.com/");
        }
    }
}