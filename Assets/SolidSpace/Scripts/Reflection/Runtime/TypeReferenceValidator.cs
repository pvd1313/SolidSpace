using System.Collections.Generic;
using SolidSpace.DataValidation;

namespace SolidSpace.Reflection
{
    [InspectorDataValidator]
    public class TypeReferenceValidator : IDataValidator<TypeReference>
    {
        private HashSet<string> _validTypes;
        
        public TypeReferenceValidator()
        {
            _validTypes = new HashSet<string>();
        }
        
        public string Validate(TypeReference data)
        {
            if (data.typeName is null)
            {
                return $"'{nameof(data.typeName)}' is null";
            }

            if (data.typeName == string.Empty)
            {
                return $"'{nameof(data.typeName)}' is empty";
            }

            if (_validTypes.Contains(data.typeName))
            {
                return string.Empty;
            }

            if (!data.TryResolve(out _))
            {
                return $"Can not resolve type '{data.typeName}'";
            }

            _validTypes.Add(data.typeName);

            return string.Empty;
        }
    }
}