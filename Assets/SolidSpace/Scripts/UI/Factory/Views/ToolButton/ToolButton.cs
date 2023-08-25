using System;
using SolidSpace.UI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    internal class ToolButton : IToolButton
    {
        public event Action Clicked;
        public VisualElement Root { get; set; }
        public VisualElement Button { get; set; }
        public VisualElement Image { get; set; }
        public IUIEventDispatcher EventDispatcher { get; set; }

        private bool _isSelected;
        
        public void OnMouseDownEvent(MouseDownEvent e)
        {
            e.StopPropagation();
            
            EventDispatcher.ScheduleOrSkip(Clicked);
        }

        public void SetSelected(bool isSelected)
        {
            if (isSelected == _isSelected)
            {
                return;
            }

            _isSelected = isSelected;
            
            if (_isSelected)
            {
                Root.AddToClassList("selected");
                Button.AddToClassList("selected");
                Image.AddToClassList("selected");
            }
            else
            {
                Root.RemoveFromClassList("selected");
                Button.RemoveFromClassList("selected");
                Image.RemoveFromClassList("selected");
            }
        }

        public void SetIcon(Sprite icon)
        {
            Image.style.backgroundImage = new StyleBackground(icon);
        }
    }
}