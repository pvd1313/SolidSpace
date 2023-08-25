using SolidSpace.DataValidation;

namespace SolidSpace.RegularExpressions
{
    [InspectorDataValidator]
    public class RegexPatternValidator : IDataValidator<RegexPattern>
    {
        public string Validate(RegexPattern data)
        {
            if (data.pattern is null)
            {
                return $"'{nameof(data.pattern)}' is null";
            }

            if (!ValidationUtil.RegexIsValid(nameof(data.pattern), data.pattern, out var message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}