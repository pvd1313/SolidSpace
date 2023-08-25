using System;
using SolidSpace.UI.Core;
using Unity.Mathematics;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    public class VerticalFixedItemList : IVerticalFixedItemList
    {
        public event Action<int> Scrolled;
        
        public VisualElement Root { get; set; }
        public VisualElement AttachPoint { get; set; }
        public VisualElement SliderStart { get; set; }
        public VisualElement SliderMiddle { get; set; }
        public VisualElement SliderEnd { get; set; }
        
        public IUIEventDispatcher EventDispatcher { get; set; }

        public void AttachItem(IUIElement item)
        {
            AttachPoint.Add(item.Root);
        }

        public void SetSliderState(int2 minMax, int2 offset)
        {
            SliderStart.style.flexGrow = new StyleFloat(Math.Max(0, offset.x - minMax.x));
            SliderMiddle.style.flexGrow = new StyleFloat(Math.Max(0, offset.y - offset.x));
            SliderEnd.style.flexGrow = new StyleFloat(Math.Max(0, minMax.y - offset.y));
        }

        public void OnWheelEvent(WheelEvent data)
        {
            var axis = Math.Sign(data.delta.y);
            EventDispatcher.ScheduleOrSkip(() => Scrolled?.Invoke(axis));
        }
    }
}