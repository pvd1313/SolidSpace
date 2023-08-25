using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    public class VerticalFixedItemListBuilder : AUIViewBuilder<VerticalFixedItemList>
    {
        private readonly IUIEventDispatcher _eventDispatcher;

        public VerticalFixedItemListBuilder(IUIEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }
        
        protected override VerticalFixedItemList Create(VisualElement root)
        {
            var view = new VerticalFixedItemList
            {
                Root = root,
                AttachPoint = UIQuery.Child<VisualElement>(root, "AttachPoint"),
                SliderStart = UIQuery.Child<VisualElement>(root, "SliderStart"),
                SliderMiddle = UIQuery.Child<VisualElement>(root, "SliderMiddle"),
                SliderEnd = UIQuery.Child<VisualElement>(root, "SliderEnd"),
                EventDispatcher = _eventDispatcher
            };
            
            view.Root.RegisterCallback<WheelEvent>(view.OnWheelEvent);
            
            return view;
        }
    }
}