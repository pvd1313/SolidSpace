using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    internal class ToolWindowValidator : IDataValidator<UIPrefab<ToolWindow>>
    {
        private readonly UITreeAssetValidator _treeValidator;
        
        public ToolWindowValidator()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        public string Validate(UIPrefab<ToolWindow> data)
        {
            if (data.Asset is null)
            {
                return $"'{nameof(data.Asset)}' is null'";
            }
            
            _treeValidator.SetAsset(data.Asset);
            
            if (!_treeValidator.TreeHasChild<VisualElement>("AttachPoint", out var message))
            {
                return message;
            }

            if (!_treeValidator.TreeHasChild<Label>("Label", out message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}