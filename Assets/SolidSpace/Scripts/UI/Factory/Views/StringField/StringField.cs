using System;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    public class StringField : IStringField
    {
        public event Action ValueChanged;
        
        public string Value { get; set; }
        public VisualElement Root { get; set; }
        
        public TextField TextField { get; set; }
        
        public bool IsValueChanged { get; set; }
        
        public IUIEventDispatcher EventDispatcher { get; set; }
        
        public IStringFieldCorrectionBehaviour CorrectionBehaviour { get; set; }

        public void SetLabel(string text)
        {
            TextField.label = text;
        }

        public void SetValue(string value)
        {
            Value = value;
            TextField.SetValueWithoutNotify(Value);
        }

        public void SetValueCorrectionBehaviour(IStringFieldCorrectionBehaviour behaviour)
        {
            CorrectionBehaviour = behaviour ?? throw new ArgumentNullException(nameof(behaviour));
        }

        public void OnValueChanged(ChangeEvent<string> eventData)
        {
            IsValueChanged = true;
        }

        public void OnFocusOut(FocusOutEvent focusOut)
        {
            if (!IsValueChanged)
            {
                return;
            }

            Value = TextField.value;
            var newValue = CorrectionBehaviour.TryFixString(Value, out var wasFixed);
            if (wasFixed)
            {
                Value = newValue;
                TextField.SetValueWithoutNotify(Value);
            }

            EventDispatcher.ScheduleOrSkip(ValueChanged);
        }
    }
}