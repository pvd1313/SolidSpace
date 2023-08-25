using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    public class GeneralButtonBuilder : AUIViewBuilder<GeneralButton>
    {
        private readonly IUIEventDispatcher _eventDispatcher;

        public GeneralButtonBuilder(IUIEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }
        
        protected override GeneralButton Create(VisualElement root)
        {
            var view = new GeneralButton
            {
                Root = root,
                Button = UIQuery.Child<VisualElement>(root, "Button"),
                Label = UIQuery.Child<Label>(root, "Label"),
                IsMouseDown = false,
                EventDispatcher = _eventDispatcher
            };
            
            view.Button.RegisterCallback<MouseDownEvent>(view.OnMouseDown);
            view.Button.RegisterCallback<MouseUpEvent>(view.OnMouseUp);
            view.Button.RegisterCallback<MouseLeaveEvent>(view.OnMouseLeave);

            return view;
        }
    }
}