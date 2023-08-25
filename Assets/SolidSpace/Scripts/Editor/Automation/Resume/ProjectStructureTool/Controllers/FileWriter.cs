using System;
using System.Linq;

namespace SolidSpace.Editor.Automation.Resume.ProjectStructureTool
{
    internal class FileWriter
    {
        private string[] _pads;
        
        public void Write(Config config, IConsole console)
        {
            var scanner = new FileScanner();
            var entities = scanner.Scan(config);
            var deepMax = entities.Max(f => f.deep);
            
            _pads = new string[deepMax + 1];
            for (var i = 0; i < _pads.Length; i++)
            {
                _pads[i] = string.Join(string.Empty, Enumerable.Repeat("|   ", i));
            }
            
            foreach (var entity in entities)
            {
                var text = FormatEntity(entity);
                console.WriteLine(text);
            }

            var fileCount = entities.Count(e => e.sizeBytes > 0);
            var directoryCount = entities.Count(e => e.sizeBytes == 0);
            var filesSizeTotal = entities.Sum(e => e.sizeBytes);
            
            console.WriteLine(string.Empty);
            console.WriteLine("Summary:");
            console.WriteLine($"\tTime: {DateTime.UtcNow:dd MMMM yyyy HH:mm:ss}");
            console.WriteLine($"\tDirectory count: {directoryCount}");
            console.WriteLine($"\tFile count: {fileCount}");
            console.WriteLine($"\tFiles size total: {FormatBytes(filesSizeTotal)}");
        }

        private string FormatEntity(EntityInfo entity)
        {
            if (entity.sizeBytes == 0)
            {
                return $"{_pads[entity.deep]}{entity.name}";
            }
            
            return $"{_pads[entity.deep]}{entity.name} ({FormatBytes(entity.sizeBytes)})";
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1000)
            {
                return $"{bytes}b";
            }
            
            var size = bytes / 1024f;
            return $"{size:N1}kb";
        }
    }
}