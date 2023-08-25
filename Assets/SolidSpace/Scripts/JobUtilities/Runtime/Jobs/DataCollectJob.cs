using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.JobUtilities
{
    [BurstCompile]
    internal struct DataCollectJob<T> : IJob where T : struct
    {
        public NativeArray<T> inOutData;

        [ReadOnly] public NativeArray<int> inCounts;
        [ReadOnly] public int inOffset;

        [WriteOnly] public NativeReference<int> outCount;
        
        public void Execute()
        {
            var resultCount = 0;
            var offsetCount = inCounts.Length;
            for (var i = 0; i < offsetCount; i++)
            {
                var localCount = inCounts[i];
                if (localCount == 0)
                {
                    continue;
                }

                var offset = i * inOffset;
                for (var j = 0; j < localCount; j++)
                {
                    inOutData[resultCount] = inOutData[offset + j];
                    resultCount++;
                }
            }

            outCount.Value = resultCount;
        }
    }
}