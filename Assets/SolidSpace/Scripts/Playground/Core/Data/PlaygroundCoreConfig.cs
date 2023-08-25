using System.Collections.Generic;
using SolidSpace.RegularExpressions;
using UnityEngine;

namespace SolidSpace.Playground.Core
{
    [System.Serializable]
    public class PlaygroundCoreConfig
    {
        public IReadOnlyList<ToolIcon> ToolIcons => _toolIcons;
        public string WindowDefaultTitle => _windowDefaultTitle;

        public RegexPatternSubstitution ToolNameConverter => _toolNameConverter;

        [SerializeField] private string _windowDefaultTitle;
        [SerializeField] private RegexPatternSubstitution _toolNameConverter;
        [SerializeField] private List<ToolIcon> _toolIcons;
    }
}