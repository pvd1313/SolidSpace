using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    internal class ToolButtonValidator : IDataValidator<UIPrefab<ToolButton>>
    {
        private readonly UITreeAssetValidator _treeValidator;

        public ToolButtonValidator()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        public string Validate(UIPrefab<ToolButton> data)
        {
            if (data.Asset is null)
            {
                return $"'{nameof(data.Asset)}' is null";
            }
            
            _treeValidator.SetAsset(data.Asset);

            if (!_treeValidator.TreeHasChild<VisualElement>("Button", out var message))
            {
                return message;
            }

            if (!_treeValidator.TreeHasChild<VisualElement>("Image", out message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}