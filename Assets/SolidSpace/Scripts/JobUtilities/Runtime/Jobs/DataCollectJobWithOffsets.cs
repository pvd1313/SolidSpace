using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.JobUtilities
{
    [BurstCompile]
    public struct DataCollectJobWithOffsets<T> : IJob 
        where T : struct
    {
        public NativeArray<T> inOutData;

        [ReadOnly] public int inDataCount;
        [ReadOnly] public NativeArray<int> inOffsets;
        [ReadOnly] public NativeArray<int> inCounts;

        [WriteOnly] public NativeReference<int> outCount;
        
        public void Execute()
        {
            var resultCount = 0;
            for (var i = 0; i < inDataCount; i++)
            {
                var localCount = inCounts[i];
                if (localCount == 0)
                {
                    continue;
                }

                var offset = inOffsets[i];
                for (var j = 0; j < localCount; j++)
                {
                    inOutData[resultCount] = inOutData[offset + j];
                    resultCount++;
                }
            }

            outCount.Value = resultCount;
        }
    }

    [BurstCompile]
    public struct DataCollectJobWithOffsets<T0, T1> : IJob 
        where T0 : struct where T1 : struct
    {
        public NativeArray<T0> inOutData0;
        public NativeArray<T1> inOutData1;

        [ReadOnly] public int inDataCount;
        [ReadOnly] public NativeArray<int> inOffsets;
        [ReadOnly] public NativeArray<int> inCounts;

        [WriteOnly] public NativeReference<int> outCount;

        public void Execute()
        {
            var resultCount = 0;
            for (var i = 0; i < inDataCount; i++)
            {
                var localCount = inCounts[i];
                if (localCount == 0)
                {
                    continue;
                }

                var offset = inOffsets[i];
                for (var j = 0; j < localCount; j++)
                {
                    inOutData0[resultCount] = inOutData0[offset + j];
                    inOutData1[resultCount] = inOutData1[offset + j];
                    resultCount++;
                }
            }

            outCount.Value = resultCount;
        }
    }

    [BurstCompile]
    public struct DataCollectJobWithOffsets<T0, T1, T2> : IJob 
        where T0 : struct where T1 : struct where T2 : struct
    {
        public NativeArray<T0> inOutData0;
        public NativeArray<T1> inOutData1;
        public NativeArray<T2> inOutData2;
        
        [ReadOnly] public int inDataCount;
        [ReadOnly] public NativeArray<int> inOffsets;
        [ReadOnly] public NativeArray<int> inCounts;

        [WriteOnly] public NativeReference<int> outCount;
        
        public void Execute()
        {
            var resultCount = 0;
            for (var i = 0; i < inDataCount; i++)
            {
                var localCount = inCounts[i];
                if (localCount == 0)
                {
                    continue;
                }

                var offset = inOffsets[i];
                for (var j = 0; j < localCount; j++)
                {
                    inOutData0[resultCount] = inOutData0[offset + j];
                    inOutData1[resultCount] = inOutData1[offset + j];
                    inOutData2[resultCount] = inOutData2[offset + j];
                    resultCount++;
                }
            }

            outCount.Value = resultCount;
        }
    }
    
    [BurstCompile]
    public struct DataCollectJobWithOffsets<T0, T1, T2, T3> : IJob 
        where T0 : struct where T1 : struct where T2 : struct where T3 : struct
    {
        public NativeArray<T0> inOutData0;
        public NativeArray<T1> inOutData1;
        public NativeArray<T2> inOutData2;
        public NativeArray<T3> inOutData3;
        
        [ReadOnly] public int inDataCount;
        [ReadOnly] public NativeArray<int> inOffsets;
        [ReadOnly] public NativeArray<int> inCounts;

        [WriteOnly] public NativeReference<int> outCount;
        
        public void Execute()
        {
            var resultCount = 0;
            for (var i = 0; i < inDataCount; i++)
            {
                var localCount = inCounts[i];
                if (localCount == 0)
                {
                    continue;
                }

                var offset = inOffsets[i];
                for (var j = 0; j < localCount; j++)
                {
                    inOutData0[resultCount] = inOutData0[offset + j];
                    inOutData1[resultCount] = inOutData1[offset + j];
                    inOutData2[resultCount] = inOutData2[offset + j];
                    inOutData3[resultCount] = inOutData3[offset + j];
                    resultCount++;
                }
            }

            outCount.Value = resultCount;
        }
    }
}