using SolidSpace.Entities.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Actors
{
    public struct FilterActivateActorsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inArchetypeChunks;
        [ReadOnly] public NativeArray<int> inWriteOffsets;
        
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<ActorComponent> actorHandle;

        [WriteOnly] public NativeArray<int> outCounts;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<float2> outPositions;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inArchetypeChunks[chunkIndex];
            var entityCount = chunk.Count;
            var writeOffset = inWriteOffsets[chunkIndex];
            var resultCount = 0;

            var positions = chunk.GetNativeArray(positionHandle);
            var actors = chunk.GetNativeArray(actorHandle);
            
            for (var i = 0; i < entityCount; i++)
            {
                var actor = actors[i];
                if (!actor.isActive)
                {
                    continue;
                }

                outPositions[writeOffset + resultCount++] = positions[i].value;
            }

            outCounts[chunkIndex] = resultCount;
        }
    }
}