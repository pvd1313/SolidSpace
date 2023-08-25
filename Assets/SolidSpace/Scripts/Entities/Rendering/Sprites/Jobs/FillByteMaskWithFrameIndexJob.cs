using SolidSpace.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Rendering.Sprites
{
    [BurstCompile]
    public struct FillByteMaskWithFrameIndexJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public ComponentTypeHandle<SpriteRenderComponent> spriteHandle;

        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<byte> outMask;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inChunks[chunkIndex];
            var entityCount = chunk.Count;
            var healthComponents = chunk.GetNativeArray(spriteHandle);
            
            for (var i = 0; i < entityCount; i++)
            {
                var atlasIndex = healthComponents[i].frameIndex;
                outMask[atlasIndex.ReadChunkId() * 64 + atlasIndex.ReadItemId()] = 1;
            }
        }
    }
}