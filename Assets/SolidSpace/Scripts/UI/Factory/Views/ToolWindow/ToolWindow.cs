using System;
using SolidSpace.UI.Core;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Factory
{
    internal class ToolWindow : IToolWindow
    {
        public VisualElement Root { get; set; }
        
        public VisualElement AttachPoint { get; set; }
        
        public Label Label { get; set; }
        
        public void AttachChild(IUIElement view)
        {
            if (view is null) throw new ArgumentNullException(nameof(view));
            
            AttachPoint.Add(view.Root);
        }

        public void SetTitle(string text)
        {
            Label.text = text;
        }
    }
}