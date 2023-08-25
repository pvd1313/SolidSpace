using SolidSpace.DataValidation;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    [InspectorDataValidator]
    public class StringFieldValidator : IDataValidator<UIPrefab<StringField>>
    {
        private readonly UITreeAssetValidator _assetValidator;
        
        public StringFieldValidator()
        {
            _assetValidator = new UITreeAssetValidator();
        }
        
        public string Validate(UIPrefab<StringField> data)
        {
            if (data.Asset is null)
            {
                return $"'{nameof(data.Asset)}' is null";
            }
            
            _assetValidator.SetAsset(data.Asset);

            if (!_assetValidator.TreeHasChild<TextField>("", out var message))
            {
                return message;
            }

            return string.Empty;
        }
    }
}