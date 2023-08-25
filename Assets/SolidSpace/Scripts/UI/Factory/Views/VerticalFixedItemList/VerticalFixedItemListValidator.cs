using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    public class VerticalFixedItemListValidator : IDataValidator<UIPrefab<VerticalFixedItemList>>
    {
        private readonly UITreeAssetValidator _treeValidator;
        
        private VerticalFixedItemListValidator()
        {
            _treeValidator = new UITreeAssetValidator();
        }
        
        public string Validate(UIPrefab<VerticalFixedItemList> data)
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

            if (!_treeValidator.TreeHasChild<VisualElement>("SliderStart", out message))
            {
                return message;
            }

            if (!_treeValidator.TreeHasChild<VisualElement>("SliderMiddle", out message))
            {
                return message;
            }

            if (!_treeValidator.TreeHasChild<VisualElement>("SliderEnd", out message))
            {
                return message;
            }
            
            return string.Empty;
        }
    }
}