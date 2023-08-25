using System.Collections.Generic;
using SolidSpace.IO.Editor;
using SolidSpace.RegularExpressions;
using UnityEngine;

namespace SolidSpace.Editor.Automation.Resume.FileCopyTool
{
    [System.Serializable]
    public class FileCopyToolConfig
    {
        public EditorFolderPath OutputFolder => _outputFolder;
        public IReadOnlyList<Object> AssetsToCopy => _assetsToCopy;
        public IReadOnlyList<RegexPattern> DeleteFilesPatterns => _deleteFilesPatterns;
        
        [SerializeField] private EditorFolderPath _outputFolder;
        [SerializeField] private List<RegexPattern> _deleteFilesPatterns;
        [SerializeField] private List<Object> _assetsToCopy;
    }
}