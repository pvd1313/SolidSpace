using System;
using SolidSpace.UI.Core;

namespace SolidSpace.UI.Factory
{
    public interface ITagLabel : IUIElement
    {
        event Action Clicked;

        void SetState(ETagLabelState newState);

        void SetLabel(string label);

        void SetLocked(bool locked);
    }
}