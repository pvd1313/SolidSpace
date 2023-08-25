using SolidSpace.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Rigidbody
{
    [BurstCompile]
    public struct RigidbodyMotionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inArchetypeChunks;
        [ReadOnly] public NativeArray<float2> inMotion;
        
        [ReadOnly] public ComponentTypeHandle<RigidbodyComponent> rigidbodyHandle;
        public ComponentTypeHandle<PositionComponent> positionHandle;

        public void Execute(int chunkIndex)
        {
            var chunk = inArchetypeChunks[chunkIndex];
            var entityCount = chunk.Count;
            var rigidbodies = chunk.GetNativeArray(rigidbodyHandle);
            var positions = chunk.GetNativeArray(positionHandle);
            
            for (var i = 0; i < entityCount; i++)
            {
                var colliderIndex = rigidbodies[i].colliderIndex;
                var position = positions[i];
                position.value += inMotion[colliderIndex];
                positions[i] = position;
            }
        }
    }
}