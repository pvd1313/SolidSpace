using SolidSpace.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Health
{
    [BurstCompile]
    public struct FillByteMaskJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public ComponentTypeHandle<HealthComponent> healthHandle;

        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<byte> outMask;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inChunks[chunkIndex];
            var entityCount = chunk.Count;
            var healthComponents = chunk.GetNativeArray(healthHandle);
            for (var i = 0; i < entityCount; i++)
            {
                var atlasIndex = healthComponents[i].index;
                outMask[atlasIndex.ReadChunkId() * 16 + atlasIndex.ReadItemId()] = 1;
            }
        }
    }
}