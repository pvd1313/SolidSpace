using System;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Core
{
    public interface IUIViewBuilder
    {
        public Type ViewType { get; }

        object Create(VisualElement source);
    }
}