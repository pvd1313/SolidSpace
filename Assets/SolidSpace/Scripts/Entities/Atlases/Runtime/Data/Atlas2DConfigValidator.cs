using SolidSpace.DataValidation;
using SolidSpace.Mathematics;

namespace SolidSpace.Entities.Atlases
{
    [InspectorDataValidator]
    internal class Atlas2DConfigValidator : IDataValidator<Atlas2DConfig>
    {
        public string Validate(Atlas2DConfig data)
        {
            if (data.MaxItemSize < data.MinItemSize)
            {
                return $"{nameof(data.MaxItemSize)} is less than {nameof(data.MinItemSize)}";
            }

            if (data.MinItemSize < AtlasMath.Min2DEntitySize)
            {
                return $"{nameof(data.MinItemSize)} must be more or equal to {AtlasMath.Min2DEntitySize}";
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

            if (data.AtlasSize < data.MaxItemSize * 4)
            {
                return $"{nameof(data.AtlasSize)} must be at least {data.MaxItemSize * 4}";
            }

            return string.Empty;
        }
    }
}