using System;
using SolidSpace.RegularExpressions;
using UnityEngine;

namespace SolidSpace.Editor.Automation.AssetNameTool
{
    [Serializable]
    internal struct FilterInfo
    {
        [SerializeField] public bool enabled;
        [SerializeField] public RegexPattern scannerFilter;
        [SerializeField] public RegexPatternSubstitution nameConverter;
    }
}