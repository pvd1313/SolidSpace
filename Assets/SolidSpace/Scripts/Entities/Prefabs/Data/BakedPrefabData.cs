using SolidSpace.Mathematics;
using Unity.Mathematics;

namespace SolidSpace.Entities.Prefabs
{
    public struct BakedPrefabData
    {
        public bool isCreated;
        public AtlasIndex16 colorIndex;
        public AtlasIndex16 bakedHealthIndex;
        public int2 size;
        public ushort entityReferenceCount;
        public bool hasManagedReference;
    }
}