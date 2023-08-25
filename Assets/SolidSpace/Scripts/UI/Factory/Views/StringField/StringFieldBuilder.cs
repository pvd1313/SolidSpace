using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    public class StringFieldBuilder : AUIViewBuilder<StringField>, IStringFieldCorrectionBehaviour
    {
        private readonly IUIEventDispatcher _eventDispatcher;

        public StringFieldBuilder(IUIEventDispatcher eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }
        
        protected override StringField Create(VisualElement root)
        {
            var view = new StringField
            {
                Root = root,
                TextField = UIQuery.Child<TextField>(root, ""),
                IsValueChanged = false,
                CorrectionBehaviour = this,
                EventDispatcher = _eventDispatcher
            };
            
            view.TextField.RegisterCallback<ChangeEvent<string>>(view.OnValueChanged);
            view.TextField.RegisterCallback<FocusOutEvent>(view.OnFocusOut);

            return view;
        }

        public string TryFixString(string value, out bool wasFixed)
        {
            wasFixed = false;
            
            return default;
        }
    }
}