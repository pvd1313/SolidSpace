using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.Entities.ParentHandle
{
    [BurstCompile]
    public struct CompareParentHandleMaskJob : IJobParallelFor
    {
        [ReadOnly, NativeDisableParallelForRestriction] public NativeSlice<HandleState> inParentHandles;
        [ReadOnly] public NativeArray<byte> inByteMask;
        [ReadOnly] public int inItemPerJob;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ushort> outWastedHandles;
        [WriteOnly] public NativeArray<int> outCounts;
        
        public void Execute(int jobIndex)
        {
            var itemOffset = jobIndex * inItemPerJob;
            var lastItem = Math.Min(itemOffset + inItemPerJob, inParentHandles.Length);
            var resultCount = 0;

            for (var i = itemOffset; i < lastItem; i++)
            {
                var handleState = inParentHandles[i];
                if (handleState.isOccupied && inByteMask[i] == 0)
                {
                    outWastedHandles[itemOffset + resultCount++] = (ushort) i;
                }
            }

            outCounts[jobIndex] = resultCount;
        }
    }
}