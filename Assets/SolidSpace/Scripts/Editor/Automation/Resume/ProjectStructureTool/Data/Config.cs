using System;
using System.Collections.Generic;
using SolidSpace.IO.Editor;
using UnityEngine;

namespace SolidSpace.Editor.Automation.Resume.ProjectStructureTool
{
    [Serializable]
    internal class Config
    {
        public EditorFolderPath ScanPath => _scanPath;
        public EditorFilePath ExportPath => _exportPath;
        public IReadOnlyList<FilterInfo> Blacklist => _blacklist;

        [SerializeField] private EditorFolderPath _scanPath;
        [SerializeField] private EditorFilePath _exportPath;
        [SerializeField] private List<FilterInfo> _blacklist;
    }
}