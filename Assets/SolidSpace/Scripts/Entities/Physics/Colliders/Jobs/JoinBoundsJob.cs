using System;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Colliders
{
    [BurstCompile]
    internal struct JoinBoundsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<FloatBounds> inBounds;
        [ReadOnly] public int inTotalBounds;
        [ReadOnly] public int inBoundsPerJob;
        
        [WriteOnly] public NativeArray<FloatBounds> outBounds;

        public void Execute(int jobIndex)
        {
            var startIndex = inBoundsPerJob * jobIndex;
            var endIndex = Math.Min(startIndex + inBoundsPerJob, inTotalBounds);
            var bounds = inBounds[startIndex];
            var xMin = bounds.xMin;
            var xMax = bounds.xMax;
            var yMin = bounds.yMin;
            var yMax = bounds.yMax;

            for (var i = startIndex + 1; i < endIndex; i++)
            {
                bounds = inBounds[i];

                if (bounds.xMin < xMin)
                {
                    xMin = bounds.xMin;
                }

                if (bounds.yMin < yMin)
                {
                    yMin = bounds.yMin;
                }

                if (bounds.xMax > xMax)
                {
                    xMax = bounds.xMax;
                }

                if (bounds.yMax > yMax)
                {
                    yMax = bounds.yMax;
                }
            }

            outBounds[jobIndex] = new FloatBounds
            {
                xMin = xMin,
                xMax = xMax,
                yMin = yMin,
                yMax = yMax
            };
        }
    }
}