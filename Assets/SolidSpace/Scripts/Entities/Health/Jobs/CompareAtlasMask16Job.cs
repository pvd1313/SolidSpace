using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.Entities.Health
{
    [BurstCompile]
    public struct CompareAtlasMask16Job : IJobParallelFor
    {
        [ReadOnly, NativeDisableParallelForRestriction] public NativeSlice<ushort> inAtlasChunksOccupation;
        [ReadOnly] public NativeArray<byte> inByteMask;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<AtlasIndex16> outRedundantIndices;
        [WriteOnly] public NativeArray<int> outCounts;
        
        public void Execute(int jobIndex)
        {
            var byteMaskOffset = jobIndex * 16;
            var atlasMask = inAtlasChunksOccupation[jobIndex];
            var resultCount = 0;

            for (var i = 0; i < 16; i++)
            {
                var requiredBit = inByteMask[byteMaskOffset + i];
                var test = ((atlasMask >> i) ^ requiredBit) & 1;
                if (test != 0)
                {
                    outRedundantIndices[byteMaskOffset + resultCount++] = new AtlasIndex16(jobIndex, i);
                }
            }

            outCounts[jobIndex] = resultCount;
        }
    }
}