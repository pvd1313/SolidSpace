using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct TimeDespawnComponent : IComponentData
    {
        public float despawnTime;
    }
}