using SolidSpace.DataValidation;

namespace SolidSpace.Profiling
{
    [InspectorDataValidator]
    public class ProfilingConfigValidator : IDataValidator<ProfilingConfig>
    {
        private const int MaxRecordCount = (1 << 16) - 2;
        
        public string Validate(ProfilingConfig data)
        {
            if (data.MaxRecordCount < 0 || data.MaxRecordCount > MaxRecordCount)
            {
                return $"{nameof(data.MaxRecordCount)} must be in range [0, {MaxRecordCount}]";
            }

            if (data.StackSize < 0)
            {
                return $"{nameof(data.StackSize)} must be more or equal zero";
            }

            return string.Empty;
        }
    }
}