using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct RigidbodyComponent : IComponentData
    {
        public ushort colliderIndex;
    }
}