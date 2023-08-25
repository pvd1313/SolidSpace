using SolidSpace.Mathematics;
using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct PrefabInstanceComponent : IComponentData
    {
        public ushort prefabIndex;
        public byte2 instanceOffset;
    }
}