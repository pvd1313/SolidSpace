using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Core
{
    [System.Serializable]
    public class UIPrefab<T> where T : class, IUIElement
    {
        public VisualTreeAsset Asset => _asset;
        
        [SerializeField] private VisualTreeAsset _asset;
    }
}