using System;
using SolidSpace.UI.Core;

namespace SolidSpace.UI.Factory
{
    public interface IGeneralButton : IUIElement
    {
        public event Action Clicked;

        void SetLabel(string text);
    }
}