using SolidSpace.RegularExpressions;
using UnityEngine;

namespace SolidSpace.Editor.Automation.Resume.ProjectStructureTool
{
    [System.Serializable]
    internal struct FilterInfo
    {
        [SerializeField] public bool enabled;
        [SerializeField] public RegexPattern filter;
    }
}