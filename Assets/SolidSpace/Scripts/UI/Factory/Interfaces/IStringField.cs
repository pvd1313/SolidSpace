using System;
using SolidSpace.UI.Core;

namespace SolidSpace.UI.Factory
{
    public interface IStringField : IUIElement
    {
        event Action ValueChanged;
        
        public string Value { get; }
        
        void SetLabel(string text);
        void SetValue(string value);
        
        void SetValueCorrectionBehaviour(IStringFieldCorrectionBehaviour behaviour);
    }
}