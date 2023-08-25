using System;
using SolidSpace.UI.Core;
using UnityEngine;

namespace SolidSpace.UI.Factory
{
    public interface IToolButton : IUIElement
    {
        public event Action Clicked;

        void SetSelected(bool isSelected);
        
        void SetIcon(Sprite icon);
    }
}