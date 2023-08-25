using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.Entities.Rendering.Sprites
{
    [BurstCompile]
    public struct CompareAtlasMask64Job : IJobParallelFor
    {
        [ReadOnly, NativeDisableParallelForRestriction] public NativeSlice<ulong> inAtlasChunksOccupation;
        [ReadOnly] public NativeArray<byte> inByteMask;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<AtlasIndex64> outRedundantIndices;
        [WriteOnly] public NativeArray<int> outCounts;
        
        public void Execute(int jobIndex)
        {
            var byteMaskOffset = jobIndex * 64;
            var atlasMask = inAtlasChunksOccupation[jobIndex];
            var resultCount = 0;

            for (var i = 0; i < 64; i++)
            {
                var requiredBit = inByteMask[byteMaskOffset + i];
                var test = ((atlasMask >> i) ^ requiredBit) & 1;
                if (test != 0)
                {
                    outRedundantIndices[byteMaskOffset + resultCount++] = new AtlasIndex64(jobIndex, i);
                }
            }

            outCounts[jobIndex] = resultCount;
        }
    }
}