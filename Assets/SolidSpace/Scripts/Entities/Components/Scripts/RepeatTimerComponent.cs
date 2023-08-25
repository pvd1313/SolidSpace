using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct RepeatTimerComponent : IComponentData
    {
        public float delay;
        public float timer;
        public byte counter;
    }
}