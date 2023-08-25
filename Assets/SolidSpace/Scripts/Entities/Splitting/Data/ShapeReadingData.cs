using Unity.Entities;
using Unity.Mathematics;

namespace SolidSpace.Entities.Splitting
{
    internal struct ShapeReadingData
    {
        public Entity entity;
        public int seedMaskOffset;
        public int healthAtlasOffset;
        public int2 entitySize;
    }
}