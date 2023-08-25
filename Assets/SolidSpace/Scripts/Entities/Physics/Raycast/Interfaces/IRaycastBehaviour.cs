using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Physics.Raycast
{
    public interface IRaycastBehaviour
    {
        void Initialize(int maxHitCount);
        void ReadChunk(ArchetypeChunk chunk);
        FloatRay GetRay(int rayIndex);
        bool TryRegisterHit(RayHit hit);
        void CollectResult(int dataCount, NativeArray<int> offsets, NativeArray<int> counts);
    }
}