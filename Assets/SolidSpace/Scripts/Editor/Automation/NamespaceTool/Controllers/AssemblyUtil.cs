using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Compilation;

namespace SolidSpace.Editor.Automation.NamespaceTool
{
    internal class AssemblyUtil
    {
        public List<AssemblyInfo> Scan(Config config)
        {
            var assemblies = new List<Assembly>();
            assemblies.AddRange(CompilationPipeline.GetAssemblies(AssembliesType.Player));
            assemblies.AddRange(CompilationPipeline.GetAssemblies(AssembliesType.Editor));

            var outAssemblies = new List<AssemblyInfo>();
            var filters = config.AssemblyFilters;
            foreach (var assembly in assemblies)
            {
                var assemblyName = assembly.name;

                for (var i = 0; i < filters.Count; i++)
                {
                    var filter = filters[i];
                    
                    if (!filter.enabled)
                    {
                        continue;
                    }

                    if (!filter.filter.IsMatch(assemblyName))
                    {
                        continue;
                    }

                    outAssemblies.Add(new AssemblyInfo
                    {
                        name = assemblyName,
                        folders = GetAssemblyFolders(assembly),
                        regexId = i
                    });
                }
            }

            var buffer = new List<string>();
            foreach (var folders in outAssemblies.Select(info => info.folders))
            {
                buffer.Clear();
                buffer.AddRange(folders);
                folders.Clear();
                foreach (var path in buffer)
                {
                    folders.Add(path.Replace('\\', '/'));
                }
            }

            return outAssemblies;
        }

        private HashSet<string> GetAssemblyFolders(Assembly assembly)
        {
            var outFolders = new HashSet<string>();
            
            foreach (var file in assembly.sourceFiles)
            {
                var folder = GetDirectoryName(file);

                while (!(folder is null))
                {
                    outFolders.Add(folder);
                    folder = GetParentFolder(folder);
                }
            }

            return outFolders;
        }

        public string AssemblyToFileName(string assemblyName)
        {
            return assemblyName + ".csproj.DotSettings";
        }
        
        private string GetParentFolder(string path)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));
            if (path == string.Empty) throw new ArgumentException($"{nameof(path)} can not be empty");
            
            var index = path.LastIndexOf('/');
            if (index == -1)
            {
                return null;
            }

            return path.Substring(0, index);
        }

        private string GetDirectoryName(string path)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));
            if (path == string.Empty) throw new ArgumentException($"{nameof(path)} can not be empty");
            
            var index = path.LastIndexOf('.');
            if (index == -1)
            {
                return path;
            }

            index = path.LastIndexOf('/');
            if (index == -1)
            {
                return path;
            }
            
            return path.Substring(0, index);
        }
    }
}