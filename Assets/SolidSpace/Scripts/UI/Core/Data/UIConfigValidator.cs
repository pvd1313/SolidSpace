using SolidSpace.DataValidation;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Core
{
    [InspectorDataValidator]
    internal class UIConfigValidator : IDataValidator<UIConfig>
    {
        private readonly VisualElement _rootAssetClone;
        
        public UIConfigValidator()
        {
            _rootAssetClone = new VisualElement();
        }
        
        public string Validate(UIConfig data)
        {
            if (data.ContainerNames is null)
            {
                return $"'{nameof(data.ContainerNames)}' is null";
            }

            if (data.RootAsset is null)
            {
                return $"'{nameof(data.RootAsset)}' is null";
            }

            if (data.PanelSettings is null)
            {
                return $"'{nameof(data.PanelSettings)}' is null";
            }

            data.RootAsset.CloneTree(_rootAssetClone);

            foreach (var containerName in data.ContainerNames)
            {
                if (string.IsNullOrEmpty(containerName))
                {
                    return $"'{nameof(data.ContainerNames)}' has container with null or empty name";
                }
                
                var element = _rootAssetClone.Query<VisualElement>(containerName).First();
                if (element is null)
                {
                    return $"{nameof(VisualElement)} '{containerName}' was not found in '{nameof(data.RootAsset)}' ({data.RootAsset.name})";
                }
            }

            return string.Empty;
        }
    }
}