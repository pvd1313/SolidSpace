using SolidSpace.DataValidation;

namespace SolidSpace.RegularExpressions
{
    [InspectorDataValidator]
    public class RegexPatternSubstitutionValidator : IDataValidator<RegexPatternSubstitution>
    {
        public string Validate(RegexPatternSubstitution data)
        {
            if (data.pattern is null)
            {
                return $"'{nameof(data.pattern)}' is null";
            }

            if (data.substitution is null)
            {
                return $"'{nameof(data.substitution)}' is null";
            }

            if (!ValidationUtil.RegexIsValid(nameof(data.pattern), data.pattern, out var message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}