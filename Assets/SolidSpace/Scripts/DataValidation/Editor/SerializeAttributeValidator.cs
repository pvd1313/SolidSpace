#if ODIN_VALIDATOR

using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using SolidSpace.DataValidation.Editor;
using UnityEngine;

[assembly: RegisterValidator(typeof(SerializeAttributeValidator))]

namespace SolidSpace.DataValidation.Editor
{
    internal class SerializeAttributeValidator : AttributeValidator<SerializeField>
    {
        private readonly HashSet<MethodInfo> _brokenValidators;
        private readonly object[] _invocationParameters;

        public SerializeAttributeValidator()
        {
            _brokenValidators = new HashSet<MethodInfo>();
            _invocationParameters = new object[] { null };
        }
        
        protected override void Initialize()
        {
            _brokenValidators.Clear();
        }

        // TODO [T-8]: Validation should give which validator reported the error.
        protected override void Validate(ValidationResult result)
        {
            if (Property.BaseValueEntry.ValueState == PropertyValueState.NullReference)
            {
                result.ResultType = ValidationResultType.Error;

                if (IsIndex(Property.NiceName))
                {
                    result.Message = $"Element at index ({Property.NiceName}) is null";
                    return;
                }

                result.Message = $"Property '{Property.NiceName}' is null";
                return;
            }

            var value = Property.BaseValueEntry.WeakSmartValue;
            var itemType = Property.BaseValueEntry.TypeOfValue;
            if (itemType.IsEnum && !Enum.IsDefined(itemType, value))
            {
                result.ResultType = ValidationResultType.Error;

                if (IsIndex(Property.NiceName))
                {
                    result.Message = $"Element at index ({Property.NiceName}) equals '{value}' which is invalid '{itemType}'";
                    return;
                }
                
                result.Message = $"Property '{Property.NiceName}' equals '{value}' which is invalid '{itemType}'";
                return;
            }

            if (!AssemblyValidatorFactory.TryGetValidatorFor(itemType, out var validationMethod))
            {
                return;
            }
            
            if (_brokenValidators.Contains(validationMethod.method))
            {
                return;
            }

            try
            {
                _invocationParameters[0] = Property.BaseValueEntry.WeakSmartValue;

                var invokeResult = (string) validationMethod.method.Invoke(validationMethod.validator, _invocationParameters);
                if (invokeResult is null)
                {
                    Debug.LogError($"Validator '{validationMethod.validator.GetType()}' returned null or non-string");
                    _brokenValidators.Add(validationMethod.method);
                    return;
                }

                if (invokeResult != string.Empty)
                {
                    result.ResultType = ValidationResultType.Error;
                    result.Message = invokeResult;
                }

            }
            catch (Exception e)
            {
                Debug.LogError($"Exception during validation via '{validationMethod.validator.GetType()}'");
                
                _brokenValidators.Add(validationMethod.method);
                
                Debug.LogException(e);
            }
        }

        private bool IsIndex(string propertyNiceName)
        {
            if (int.TryParse(propertyNiceName, out _))
            {
                return true;
            }

            return false;
        }
    }
}

#endif