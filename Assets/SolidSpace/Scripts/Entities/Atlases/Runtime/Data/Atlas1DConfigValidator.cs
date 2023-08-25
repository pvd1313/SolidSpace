using SolidSpace.DataValidation;
using SolidSpace.Mathematics;

namespace SolidSpace.Entities.Atlases
{
    [InspectorDataValidator]
    public class Atlas1DConfigValidator : IDataValidator<Atlas1DConfig>
    {
        public string Validate(Atlas1DConfig data)
        {
            if (data.MaxItemSize < data.MinItemSize)
            {
                return $"{nameof(data.MaxItemSize)} is less than {nameof(data.MinItemSize)}";
            }

            if (data.MinItemSize < AtlasMath.Min1DEntitySize)
            {
                return $"{nameof(data.MinItemSize)} must be more or equal to {AtlasMath.Min1DEntitySize}";
            }

            if (!BinaryMath.IsPowerOfTwo(data.MinItemSize))
            {
                return $"{nameof(data.MinItemSize)} is not power of 2";
            }

            if (!BinaryMath.IsPowerOfTwo(data.MaxItemSize))
            {
                return $"{nameof(data.MaxItemSize)} is not power of 2";
            }

            if (!BinaryMath.IsPowerOfTwo(data.AtlasSize))
            {
                return $"{nameof(data.AtlasSize)} is not power of 2";
            }

            if (data.AtlasSize < data.MaxItemSize * 16)
            {
                return $"{nameof(data.AtlasSize)} must be at least {data.MaxItemSize * 16}";
            }

            return string.Empty;
        }
    }
}