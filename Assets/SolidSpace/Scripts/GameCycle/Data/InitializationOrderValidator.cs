using System;
using System.Collections.Generic;
using SolidSpace.DataValidation;
using SolidSpace.Reflection;

namespace SolidSpace.GameCycle
{
    [InspectorDataValidator]
    public class InitializationOrderValidator : IDataValidator<InitializationOrder>
    {
        private readonly HashSet<TypeReference> _controllers;
        private readonly HashSet<string> _groupNames;
        private readonly HashSet<TypeReference> _validTypes;
        private readonly Type _initializableType;

        public InitializationOrderValidator()
        {
            _controllers = new HashSet<TypeReference>();
            _groupNames = new HashSet<string>();
            _validTypes = new HashSet<TypeReference>();
            _initializableType = typeof(IInitializable);
        }
        
        public string Validate(InitializationOrder data)
        {
            if (data.Groups is null)
            {
                return $"'{nameof(data.Groups)}' is null";
            }

            _groupNames.Clear();
            _controllers.Clear();

            for (var i = 0; i < data.Groups.Count; i++)
            {
                var group = data.Groups[i];
                if (!_groupNames.Add(group.Name))
                {
                    return $"Group name '{group.Name}' is duplicated";
                }
                
                if (group.Controllers is null)
                {
                    return $"{nameof(group.Controllers)} is null at group ({i}) '{group.Name}'";
                }
                
                for (var j = 0; j < group.Controllers.Count; j++)
                {
                    var controllerName = group.Controllers[j];
                    if (!_controllers.Add(controllerName))
                    {
                        return $"Controller name '{controllerName}' is duplicated";
                    }

                    if (_validTypes.Contains(controllerName))
                    {
                        continue;
                    }
                    
                    if (!controllerName.TryResolve(out var type))
                    {
                        return $"Type for controller with name '{controllerName}' was not found";
                    }

                    if (!_initializableType.IsAssignableFrom(type))
                    {
                        return $"Controller '{controllerName}' does not implement {nameof(IInitializable)}";
                    }
                    
                    _validTypes.Add(controllerName);
                }
            }
            
            return string.Empty;
        }
    }
}