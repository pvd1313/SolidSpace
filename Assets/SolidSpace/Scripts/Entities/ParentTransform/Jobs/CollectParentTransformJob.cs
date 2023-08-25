using SolidSpace.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.ParentTransform
{
    [BurstCompile]
    public struct CollectParentTransformJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;

        [ReadOnly] public ComponentTypeHandle<ParentComponent> parentHandle;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<RotationComponent> rotationHandle;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ParentData> outParentData;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inChunks[chunkIndex];
            var entityCount = chunk.ChunkEntityCount;
            var positions = chunk.GetNativeArray(positionHandle);
            var parents = chunk.GetNativeArray(parentHandle);

            if (chunk.Has(rotationHandle))
            {
                var rotations = chunk.GetNativeArray(rotationHandle);

                for (var i = 0; i < entityCount; i++)
                {
                    var handle = parents[i].handle;
                    outParentData[handle.index] = new ParentData
                    {
                        position = positions[i].value,
                        rotation = rotations[i].value
                    };
                }
                
                return;
            }

            for (var i = 0; i < entityCount; i++)
            {
                var handle = parents[i].handle;
                outParentData[handle.index] = new ParentData
                {
                    position = positions[i].value,
                    rotation = 0,
                };
            }
        }
    }
}