using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.Components
{
    public struct LocalPositionComponent : IComponentData
    {
        public float2 value;
    }
}