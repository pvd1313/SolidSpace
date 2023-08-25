using System;
using System.Collections.Generic;
using SolidSpace.GameCycle;

namespace SolidSpace.UI.Core
{
    public class UIEventDispatcher : IInitializable, IUpdatable, IUIEventDispatcher
    {
        private List<Action> _actions;
        
        public void OnInitialize()
        {
            _actions = new List<Action>();
        }

        public void OnUpdate()
        {
            foreach (var action in _actions)
            {
                action.Invoke();
            }
            
            _actions.Clear();
        }

        public void ScheduleOrSkip(Action action)
        {
            if (action is null)
            {
                return;
            }
            
            _actions.Add(action);
        }
        
        public void OnFinalize()
        {
            
        }
    }
}