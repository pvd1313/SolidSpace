using System;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    public class GeneralButton : IGeneralButton
    {
        public event Action Clicked;
        
        public VisualElement Root { get; set; }
        public VisualElement Button { get; set; }
        public Label Label { get; set; }
        public bool IsMouseDown { get; set; }
        
        public IUIEventDispatcher EventDispatcher { get; set; }

        public void SetLabel(string text)
        {
            Label.text = text;
        }
        public void OnMouseLeave(MouseLeaveEvent e)
        {
            if (IsMouseDown)
            {
                IsMouseDown = false;
                
                RemoveFromClassList("pressed");
            }
        }

        public void OnMouseDown(MouseDownEvent e)
        {
            e.StopPropagation();
            
            AddToClassList("pressed");

            IsMouseDown = true;
        }

        public void OnMouseUp(MouseUpEvent e)
        {
            if (IsMouseDown)
            {
                IsMouseDown = false;
                
                RemoveFromClassList("pressed");
                
                EventDispatcher.ScheduleOrSkip(Clicked);
            }
        }

        public void AddToClassList(string className)
        {
            Root.AddToClassList(className);
            Button.AddToClassList(className);
            Label.AddToClassList(className);
        }

        public void RemoveFromClassList(string className)
        {
            Root.RemoveFromClassList(className);
            Button.RemoveFromClassList(className);
            Label.RemoveFromClassList(className);
        }
    }
}