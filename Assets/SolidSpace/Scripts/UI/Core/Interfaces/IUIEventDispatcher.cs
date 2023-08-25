using System;

namespace SolidSpace.UI.Core
{
    public interface IUIEventDispatcher
    {
        void ScheduleOrSkip(Action action);
    }
}