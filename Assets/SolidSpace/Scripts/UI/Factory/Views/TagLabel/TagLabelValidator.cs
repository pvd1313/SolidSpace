using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    internal class TagLabelValidator : IDataValidator<UIPrefab<TagLabel>>
    {
        private readonly UITreeAssetValidator _treeValidator;
        
        public TagLabelValidator()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        public string Validate(UIPrefab<TagLabel> data)
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
            
            if (!_treeValidator.TreeHasChild<VisualElement>("Lock", out message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}