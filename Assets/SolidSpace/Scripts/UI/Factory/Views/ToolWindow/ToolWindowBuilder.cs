using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    internal class ToolWindowBuilder : AUIViewBuilder<ToolWindow>
    {
        protected override ToolWindow Create(VisualElement root)
        {
            return new ToolWindow
            {
                Root = root,
                AttachPoint = UIQuery.Child<VisualElement>(root, "AttachPoint"),
                Label = UIQuery.Child<Label>(root, "Label")
            };
        }
    }
}