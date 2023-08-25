using SolidSpace.Entities.Splitting;
using SolidSpace.Mathematics;
using Unity.Entities;

namespace SolidSpace.Entities.Prefabs
{
    public struct PrefabReplicationData
    {
        public Entity parent;
        public AtlasIndex16 childHealth;
        public ByteBounds childBounds;
    }
}