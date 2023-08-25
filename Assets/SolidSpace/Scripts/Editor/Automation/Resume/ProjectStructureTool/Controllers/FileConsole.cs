using System;
using System.IO;

namespace SolidSpace.Editor.Automation.Resume.ProjectStructureTool
{
    internal class FileConsole : IConsole, IDisposable
    {
        private readonly StreamWriter _writer;

        public FileConsole(string path, bool clearFile)
        {
            _writer = new StreamWriter(path, !clearFile);
        }
        
        public void WriteLine(string text)
        {
            _writer.WriteLine(text);
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}