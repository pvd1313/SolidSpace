using SolidSpace.Mathematics;
using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.Splitting
{
    public struct SplittingEntityData
    {
        public Entity entity;
        public AtlasIndex16 health;
        public int2 size;
    }
}