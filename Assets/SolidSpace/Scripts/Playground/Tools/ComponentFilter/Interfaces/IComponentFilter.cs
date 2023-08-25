using System;
using System.Collections.Generic;
using SolidSpace.UI.Core;
using SolidSpace.UI.Factory;
using Unity.Entities;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    public interface IComponentFilter : IUIElement
    {
        event Action FilterModified;

        IEnumerable<ComponentType> GetTagsWithState(ETagLabelState state);
    }
}