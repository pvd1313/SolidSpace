using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Profiling
{
    [BurstCompile]
    internal struct ProfilingReadTreeJob : IJob
    {
        [ReadOnly] public NativeArray<ushort> inChilds;
        [ReadOnly] public NativeArray<ushort> inSiblings;
        [ReadOnly] public NativeArray<ushort> inNames;
        [ReadOnly] public NativeArray<float> inTimes;
        [ReadOnly] public int2 inReadRange;

        [WriteOnly] public NativeArray<NativeNode> outNodes;
        [WriteOnly] public NativeArray<int> outNodeCount;
        [WriteOnly] public NativeArray<int> outTotalNodeCount;

        private int _bakedNodeCount;
        private int _nodeIndexLinear;
        
        public void Execute()
        {
            _nodeIndexLinear = -1;
            _bakedNodeCount = 0;

            ReadNodeRecursive(0, 0);

            outNodeCount[0] = _bakedNodeCount;
            outTotalNodeCount[0] = _nodeIndexLinear + 1;
        }

        private void ReadNodeRecursive(int nodeIndex, int deep)
        {
            _nodeIndexLinear++;

            if (_nodeIndexLinear >= inReadRange.x && _nodeIndexLinear <= inReadRange.y)
            {
                outNodes[_bakedNodeCount++] = new NativeNode
                {
                    name = inNames[nodeIndex],
                    time = inTimes[nodeIndex],
                    deep = deep
                };
            }

            var siblingIndex = inChilds[nodeIndex];
            
            while (siblingIndex != 0)
            {
                ReadNodeRecursive(siblingIndex, deep + 1);

                siblingIndex = inSiblings[siblingIndex];
            }
        }
    }
}