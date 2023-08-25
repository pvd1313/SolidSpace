using SolidSpace.UI.Core;
using UnityEngine;

namespace SolidSpace.UI.Factory
{
    [System.Serializable]
    internal class UIPrefabs
    {
        public UIPrefab<ToolButton> ToolButton => _toolButton;
        public UIPrefab<ToolWindow> ToolWindow => _toolWindow;
        public UIPrefab<TagLabel> TagLabel => _tagLabel;
        public UIPrefab<LayoutGrid> LayoutGrid => _layoutGrid;
        public UIPrefab<GeneralButton> GeneralButton => _generalButton;
        public UIPrefab<StringField> StringField => _stringField;
        public UIPrefab<VerticalFixedItemList> VerticalFixedItemList => _verticalFixedItemList;
        
        [SerializeField] private UIPrefab<ToolButton> _toolButton;
        [SerializeField] private UIPrefab<ToolWindow> _toolWindow;
        [SerializeField] private UIPrefab<TagLabel> _tagLabel;
        [SerializeField] private UIPrefab<LayoutGrid> _layoutGrid;
        [SerializeField] private UIPrefab<GeneralButton> _generalButton;
        [SerializeField] private UIPrefab<StringField> _stringField;
        [SerializeField] private UIPrefab<VerticalFixedItemList> _verticalFixedItemList;
    }
}