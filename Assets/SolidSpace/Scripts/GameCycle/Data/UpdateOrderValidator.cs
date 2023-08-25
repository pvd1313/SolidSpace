using System;
using System.Collections.Generic;
using SolidSpace.DataValidation;
using SolidSpace.Reflection;

namespace SolidSpace.GameCycle
{
    [InspectorDataValidator]
    public class UpdateOrderValidator : IDataValidator<UpdateOrder>
    {
        private readonly HashSet<TypeReference> _controllers;
        private readonly HashSet<string> _groupNames;
        private readonly HashSet<TypeReference> _validTypes;
        private readonly Type _updatableType;

        public UpdateOrderValidator()
        {
            _controllers = new HashSet<TypeReference>();
            _groupNames = new HashSet<string>();
            _validTypes = new HashSet<TypeReference>();
            _updatableType = typeof(IUpdatable);
        }
        
        public string Validate(UpdateOrder data)
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
                    var controller = group.Controllers[j];
                    if (!_controllers.Add(controller))
                    {
                        return $"Controller name '{controller}' is duplicated";
                    }

                    if (_validTypes.Contains(controller))
                    {
                        continue;
                    }

                    if (!controller.TryResolve(out var type))
                    {
                        return $"Type for controller with name '{controller}' was not found";
                    }

                    if (!_updatableType.IsAssignableFrom(type))
                    {
                        return $"Controller '{controller}' does not implement {nameof(IUpdatable)}";
                    }
                    
                    _validTypes.Add(controller);
                }
            }
            
            return string.Empty;
        }
    }
}