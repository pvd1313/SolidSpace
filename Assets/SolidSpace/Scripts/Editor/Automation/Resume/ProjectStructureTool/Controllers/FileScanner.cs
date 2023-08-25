using System.Collections.Generic;
using System.IO;
using System.Linq;
using SolidSpace.IO.Editor;
using SolidSpace.RegularExpressions;

namespace SolidSpace.Editor.Automation.Resume.ProjectStructureTool
{
    internal class FileScanner
    {
        private readonly List<RegexPattern> _blacklistFilters; 
        
        private List<EntityInfo> _outEntities;

        public FileScanner()
        {
            _blacklistFilters = new List<RegexPattern>();
        }

        public List<EntityInfo> Scan(Config config)
        {
            _outEntities = new List<EntityInfo>();

            _blacklistFilters.Clear();
            _blacklistFilters.AddRange(config.Blacklist.Where(f => f.enabled).Select(f => f.filter));

            var scanRoot = EditorPath.Combine(EditorPath.ProjectRoot, config.ScanPath.path);
            ScanRecursive(scanRoot, 0);
            
            return _outEntities;
        }

        private void ScanRecursive(string path, int deep)
        {
            if (_blacklistFilters.Any(filter => filter.IsMatch(path)))
            {
                return;
            }
            
            _outEntities.Add(new EntityInfo
            {
                name = Path.GetFileName(path),
                deep = deep,
                sizeBytes = 0
            });

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                ScanRecursive(directory, deep + 1);
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (_blacklistFilters.Any(filter => filter.IsMatch(file)))
                {
                    continue;
                }

                var fileInfo = new FileInfo(file);
                
                _outEntities.Add(new EntityInfo
                {
                    name = Path.GetFileName(file),
                    deep = deep + 1,
                    sizeBytes = fileInfo.Length
                });
            }
        }
    }
}