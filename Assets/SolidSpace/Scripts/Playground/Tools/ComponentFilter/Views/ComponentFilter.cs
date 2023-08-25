using System;
using System.Collections.Generic;
using SolidSpace.UI.Factory;
using Unity.Entities;
using UnityEngine.UIElements;

namespace SolidSpace.Playground.Tools.ComponentFilter
{
    internal class ComponentFilter : IComponentFilter
    {
        public event Action FilterModified;

        public VisualElement Root { get; set; }
        public FilterState[] Filter { get; set; }
        public ComponentType[] AllComponents { get; set; }
        public Dictionary<ComponentType, int> ComponentToIndex { get; set; }
        public ITagLabel[] Tags { get; set; }

        public void SetState(ComponentType componentType, FilterState state)
        {
            var index = ComponentToIndex[componentType];
            var oldState = Filter[index];
            if (oldState.isLocked != state.isLocked || oldState.state != state.state)
            {
                Filter[index] = state;
                var tag = Tags[index];
                tag.SetLocked(state.isLocked);
                tag.SetState(state.state);
            }
        }

        public IEnumerable<ComponentType> GetTagsWithState(ETagLabelState state)
        {
            for (var i = 0; i < AllComponents.Length; i++)
            {
                if (Filter[i].state == state)
                {
                    yield return AllComponents[i];
                }
            }
        }

        public void OnTagClicked(int index)
        {
            var filterState = Filter[index];
            if (filterState.isLocked)
            {
                return;
            }

            filterState.state = AdvanceState(filterState.state);
            Filter[index] = filterState;
            Tags[index].SetState(filterState.state);
            
            FilterModified?.Invoke();
        }
        
        private ETagLabelState AdvanceState(ETagLabelState state)
        {
            return state switch
            {
                ETagLabelState.Neutral => ETagLabelState.Positive,
                ETagLabelState.Positive => ETagLabelState.Negative,
                ETagLabelState.Negative => ETagLabelState.Neutral,
                
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
    }
}