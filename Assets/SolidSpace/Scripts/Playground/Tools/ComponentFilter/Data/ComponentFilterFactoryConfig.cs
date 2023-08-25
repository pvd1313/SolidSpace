using System;
using SolidSpace.RegularExpressions;
using UnityEngine;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    [Serializable]
    public class ComponentFilterFactoryConfig
    {
        public RegexPattern Filter => _filter;
        public RegexPatternSubstitution NameConverter => _nameConverter;

        [SerializeField] private RegexPattern _filter;
        [SerializeField] private RegexPatternSubstitution _nameConverter;
    }
}