using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.Components
{
    public struct VelocityComponent : IComponentData
    {
        public float2 value;
    }
}