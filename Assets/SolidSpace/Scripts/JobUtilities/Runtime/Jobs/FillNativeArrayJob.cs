using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.JobUtilities
{
    [BurstCompile]
    internal struct FillNativeArrayJob<T> : IJobParallelFor where T : struct
    {
        [ReadOnly] public T inValue;
        [ReadOnly] public int inItemPerJob;
        [ReadOnly] public int inTotalItem;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<T> outNativeArray;
        
        public void Execute(int jobIndex)
        {
            var startIndex = jobIndex * inItemPerJob;
            var endIndex = Math.Min(startIndex + inItemPerJob, inTotalItem);
            for (var i = startIndex; i < endIndex; i++)
            {
                outNativeArray[i] = inValue;
            }
        }
    }
}