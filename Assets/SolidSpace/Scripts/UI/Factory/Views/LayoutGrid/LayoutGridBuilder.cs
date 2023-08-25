using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    internal class LayoutGridBuilder : AUIViewBuilder<LayoutGrid>
    {
        protected override LayoutGrid Create(VisualElement root)
        {
            return new LayoutGrid
            {
                Root = root
            };
        }
    }
}