using System.Collections.Generic;
using UnityEngine;

namespace SolidSpace.Editor.Automation.Resume.CommandLineTool
{
    [System.Serializable]
    public class CommandLineToolConfig
    {
        public IReadOnlyList<string> CommandLines => _commandLines;
        
        [SerializeField] private List<string> _commandLines;
    }
}