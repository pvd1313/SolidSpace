using System;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Core
{
    internal readonly struct IUICallbackHandler<T> : IUICallbackHandler where T : EventBase<T>, new()
    {
        private readonly Action<T> _action;
        
        public IUICallbackHandler(Action<T> action)
        {
            _action = action;
        }
        
        public void Invoke(object eventData)
        {
            _action.Invoke((T) eventData);
        }
    }
}