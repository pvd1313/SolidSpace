using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SolidSpace.RegularExpressions
{
    [Serializable]
    public struct RegexPatternSubstitution
    {
        [SerializeField] public string pattern;
        [SerializeField] public string substitution;

        public string Replace(string text)
        {
            return Regex.Replace(text, pattern, substitution);
        }
    }
}