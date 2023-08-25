using SolidSpace.Mathematics;
using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct HealthComponent : IComponentData
    {
        public AtlasIndex16 index;
    }
}