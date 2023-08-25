using SolidSpace.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Despawn
{
    [BurstCompile] 
    internal struct TimeDespawnComputeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public NativeArray<int> inWriteOffsets;
        
        [ReadOnly] public ComponentTypeHandle<TimeDespawnComponent> despawnHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;
        [ReadOnly] public float time;
        
        [WriteOnly] public NativeArray<int> outEntityCounts;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Entity> outEntities;

        public void Execute(int chunkIndex)
        {
            var chunk = inChunks[chunkIndex];
            var writeOffset = inWriteOffsets[chunkIndex];
            var entityCount = chunk.Count;
            var despawns = chunk.GetNativeArray(despawnHandle);
            var entities = chunk.GetNativeArray(entityHandle);
            var resultCount = 0;

            for (var i = 0; i < entityCount; i++)
            {
                if (time < despawns[i].despawnTime)
                {
                    continue;
                }

                outEntities[writeOffset + resultCount] = entities[i];
                resultCount++;
            }

            outEntityCounts[chunkIndex] = resultCount;
        }
    }
}