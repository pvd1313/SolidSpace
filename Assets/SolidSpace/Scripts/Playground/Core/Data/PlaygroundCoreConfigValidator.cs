using System.Collections.Generic;
using SolidSpace.DataValidation;
using SolidSpace.Reflection;

namespace SolidSpace.Playground.Core
{
    [InspectorDataValidator]
    public class PlaygroundCoreConfigValidator : IDataValidator<PlaygroundCoreConfig>
    {
        private readonly HashSet<TypeReference> _typeCash;
        
        public PlaygroundCoreConfigValidator()
        {
            _typeCash = new HashSet<TypeReference>();
        }
        
        public string Validate(PlaygroundCoreConfig data)
        {
            if (data.ToolIcons is null)
            {
                return $"'{nameof(data.ToolIcons)}' is null";
            }
            
            _typeCash.Clear();

            var icons = data.ToolIcons;
            for (var i = 0; i < icons.Count; i++)
            {
                var toolType = icons[i].toolType;
                if (!_typeCash.Add(toolType))
                {
                    return $"Tool '{toolType}' is duplicated";
                }
            }

            return string.Empty;
        }
    }
}