using System;
using System.Text.RegularExpressions;

namespace SolidSpace.RegularExpressions
{
    [Serializable]
    public struct RegexPattern
    {
        public string pattern;

        public bool IsMatch(string text)
        {
            return Regex.IsMatch(text, pattern);
        }
    }
}