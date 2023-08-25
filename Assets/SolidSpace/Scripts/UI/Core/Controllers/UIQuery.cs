using System;
using UnityEngine.UIElements;

namespace SolidSpace.UI.Core
{
    public static class UIQuery
    {
        public static T Child<T>(VisualElement root, string childName) where T : VisualElement
        {
            var child = root.Query<T>(childName).First();
            if (child is null)
            {
                throw new InvalidOperationException($"'{childName}' ({typeof(T)}) was not found.");
            }

            return child;
        }
    }
}