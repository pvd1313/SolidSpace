using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Colliders
{
    [BurstCompile]
    internal struct ChunkListsFillJob : IJob
    {
        [ReadOnly] public NativeArray<ChunkedCollider> inColliders;
        [ReadOnly] public int inColliderBatchCapacity;
        [ReadOnly] public NativeArray<int> inColliderCounts;

        public NativeArray<ColliderListPointer> inOutLists;
        
        [WriteOnly] public NativeArray<ushort> outColliders;
        
        public void Execute()
        {
            var colliderChunkCount = inColliderCounts.Length;
            var readOffset = 0;
            for (var chunkIndex = 0; chunkIndex < colliderChunkCount; chunkIndex++)
            {
                var colliderCount = inColliderCounts[chunkIndex];
                var lastCollider = readOffset + colliderCount;
                for (var i = readOffset; i < lastCollider; i++)
                {
                    var collider = inColliders[i];
                    var list = inOutLists[collider.chunkIndex];
                    outColliders[list.offset + list.wroteCount] = collider.colliderIndex;
                    list.wroteCount++;
                    inOutLists[collider.chunkIndex] = list;
                }

                readOffset += inColliderBatchCapacity;
            }
        }
    }
}