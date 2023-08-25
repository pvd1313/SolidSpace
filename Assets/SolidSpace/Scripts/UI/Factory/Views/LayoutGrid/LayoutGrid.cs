using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    internal class LayoutGrid : ILayoutGrid
    {
        public VisualElement Root { get; set; }

        public void AttachChild(IUIElement child)
        {
            Root.Add(child.Root);
        }

        public void SetFlexWrap(Wrap wrap)
        {
            Root.style.flexWrap = wrap;
        }

        public void SetFlexDirection(FlexDirection direction)
        {
            Root.style.flexDirection = direction;
        }

        public void SetAlignItems(Align align)
        {
            Root.style.alignItems = align;
        }

        public void SetJustifyContent(Justify justify)
        {
            Root.style.justifyContent = justify;
        }
    }
}