using SolidSpace.UI.Core;
using System;
using Unity.Mathematics;

namespace SolidSpace.UI.Factory
{
    public interface IVerticalFixedItemList : IUIElement
    {
        public event Action<int> Scrolled;

        void AttachItem(IUIElement item);

        void SetSliderState(int2 minMax, int2 offset);
    }
}