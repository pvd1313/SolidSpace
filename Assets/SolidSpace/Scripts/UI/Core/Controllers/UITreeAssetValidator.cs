using System;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Core
{
    public class UITreeAssetValidator
    {
        private VisualElement _clone;
        
        private VisualTreeAsset _asset;

        public UITreeAssetValidator()
        {
            _clone = new VisualElement();
        }

        public void SetAsset(VisualTreeAsset asset)
        {
            if (!asset) throw new ArgumentNullException(nameof(asset));

            _asset = asset;
            _clone = _asset.CloneTree();
        }

        public bool TreeHasChild<T>(string childName, out string errorMessage) where T : VisualElement
        {
            if (!_asset) throw new InvalidOperationException($"{nameof(SetAsset)}() was not called or asset is destroyed");
            
            errorMessage = string.Empty;
            
            var child = _clone.Query<T>(childName).First();
            if (child is null)
            {
                errorMessage = $"Tree '{_asset.name}' does not have child '{childName}' ({typeof(T)})";
                return false;
            }

            return true;
        }
    }
}