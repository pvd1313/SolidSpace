using System;
using System.Text.RegularExpressions;

namespace SolidSpace.DataValidation
{
    public static class ValidationUtil
    {
        public static bool RegexIsValid(string propertyName, string regex, out string message)
        {
            message = string.Empty;
            
            try
            {
                Regex.IsMatch("", regex);
            }
            catch (Exception e)
            {
                message = $"'{propertyName}' is invalid: {e.Message}";
                return false;
            }

            return true;
        }
    }
}