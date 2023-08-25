using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    public class GeneralButtonValidator : IDataValidator<UIPrefab<GeneralButton>>
    {
        private readonly UITreeAssetValidator _treeValidator;
        
        public GeneralButtonValidator()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        public string Validate(UIPrefab<GeneralButton> data)
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

            if (!_treeValidator.TreeHasChild<Label>("Label", out message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}