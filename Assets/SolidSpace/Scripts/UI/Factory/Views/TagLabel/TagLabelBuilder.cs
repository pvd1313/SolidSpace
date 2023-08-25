using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    internal class TagLabelBuilder : AUIViewBuilder<TagLabel>
    {
        private readonly IUIEventDispatcher _eventDispatcher;

        public TagLabelBuilder(IUIEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        protected override TagLabel Create(VisualElement root)
        {
            var view = new TagLabel
            {
                Root = root,
                Button = UIQuery.Child<VisualElement>(root, "Button"),
                Label = UIQuery.Child<Label>(root, "Label"),
                Lock = UIQuery.Child<VisualElement>(root, "Lock"),
                State = ETagLabelState.Neutral,
                IsLocked = false,
                EventDispatcher = _eventDispatcher
            };
            
            view.AddToClassList(TagLabel.StateToName(ETagLabelState.Neutral));
            view.AddToClassList("unlocked");
            
            view.Button.RegisterCallback<MouseDownEvent>(view.OnMouseDownEvent);

            return view;
        }
    }
}