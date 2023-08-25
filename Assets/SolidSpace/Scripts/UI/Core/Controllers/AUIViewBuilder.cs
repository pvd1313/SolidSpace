using System;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Core
{
    public abstract class AUIViewBuilder<T> : IUIViewBuilder where T : class, IUIElement
    {
        public Type ViewType => typeof(T);

        object IUIViewBuilder.Create(VisualElement root)
        {
            return Create(root);
        }

        protected abstract T Create(VisualElement root);
    }
}