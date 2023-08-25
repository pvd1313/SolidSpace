using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    internal class ToolButtonBuilder : AUIViewBuilder<ToolButton>
    {
        private readonly IUIEventDispatcher _eventDispatcher;
        
        public ToolButtonBuilder(IUIEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        protected override ToolButton Create(VisualElement root)
        {
            var view = new ToolButton
            {
                Root = root,
                Button = UIQuery.Child<VisualElement>(root, "Button"),
                Image = UIQuery.Child<VisualElement>(root, "Image"),
                EventDispatcher = _eventDispatcher
            };

            view.Button.RegisterCallback<MouseDownEvent>(view.OnMouseDownEvent);

            return view;
        }
    }
}