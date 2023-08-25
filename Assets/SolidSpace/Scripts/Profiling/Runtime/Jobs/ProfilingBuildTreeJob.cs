using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.Profiling
{
    [BurstCompile]
    internal struct ProfilingBuildTreeJob : IJob
    {
        [ReadOnly] public NativeArray<ProfilingRecord> inRecords;
        [ReadOnly] public int inRecordCount;
        [ReadOnly] public long inFrequency;

        public NativeArray<ushort> parentStack;
        public NativeArray<ushort> siblingStack;
        public NativeArray<int> nameHashStack;
        public NativeArray<int> timeStack;

        public NativeArray<ushort> outChilds;
        [WriteOnly] public NativeArray<ushort> outSiblings;
        [WriteOnly] public NativeArray<ushort> outNames;
        [WriteOnly] public NativeArray<float> outTimes;
        [WriteOnly] public NativeArray<TreeBuildState> outState;

        private int _stackLast;
        private int _nodeCount;
        private int _stackMax;
        private long _ticksTotal;
        private float _ticksToMilliseconds;
        private ETreeBuildCode _code;

        public void Execute()
        {
            _ticksToMilliseconds = 1000f / inFrequency;
            _stackLast = 0;
            _nodeCount = 1;
            parentStack[0] = 0;
            siblingStack[0] = 0;
            nameHashStack[0] = 0;
            outChilds[0] = 0;
            outNames[0] = 0;
            outSiblings[0] = 0;
            outTimes[0] = 0;
            outState[0] = new TreeBuildState
            {
                code = ETreeBuildCode.Unknown
            };
            _code = ETreeBuildCode.Unknown;
            _stackMax = Math.Min(parentStack.Length, siblingStack.Length);
            
            for (var i = 0; i < inRecordCount; i++)
            {
                var record = inRecords[i];
                
                record.Read(out var timeSamples, out var isBeginSampleCommand, out var nameHash);
                
                if (isBeginSampleCommand)
                {
                    FlushBeginSample(i + 1, timeSamples, nameHash);
                }
                else
                {
                    FlushEndSample(timeSamples, nameHash);
                }

                if (_code != ETreeBuildCode.Unknown)
                {
                    outState[0] = new TreeBuildState
                    {
                        code = _code,
                        stackLast = _stackLast,
                        recordLast = i
                    };
     
                    return;
                }
            }

            if (_stackLast != 0)
            {
                outState[0] = new TreeBuildState
                {
                    code = ETreeBuildCode.StackIsNotEmptyAfterJobComplete,
                    stackLast = _stackLast
                };

                return;
            }

            outTimes[0] = _ticksTotal * _ticksToMilliseconds;
            outState[0] = new TreeBuildState
            {
                code = ETreeBuildCode.Success
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushBeginSample(int nameIndex, int timeSamples, int nameHash)
        {
            if (_stackLast + 1 >= _stackMax)
            {
                _code = ETreeBuildCode.StackOverflow;
                return;
            }
            
            var nodeIndex =  (ushort) _nodeCount++;
            var siblingIndex = siblingStack[_stackLast];

            if (siblingIndex == 0)
            {
                var parentIndex = parentStack[_stackLast];
                outChilds[parentIndex] = nodeIndex;
            }
            else
            {
                outSiblings[siblingIndex] = nodeIndex;
            }

            siblingStack[_stackLast++] = nodeIndex;
            siblingStack[_stackLast] = 0;
            nameHashStack[_stackLast] = nameHash;
            parentStack[_stackLast] = nodeIndex;
            timeStack[_stackLast] = timeSamples;

            outNames[nodeIndex] = (ushort) nameIndex;
            outChilds[nodeIndex] = 0;
            outSiblings[nodeIndex] = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushEndSample(int timeSamples, int nameHash)
        {
            if (_stackLast == 0)
            {
                _code = ETreeBuildCode.StackUnderflow;
                return;
            }

            if (nameHash != nameHashStack[_stackLast])
            {
                _code = ETreeBuildCode.NameMismatch;
                return;
            }

            var nodeIndex = parentStack[_stackLast];
            var ticks = timeSamples - timeStack[_stackLast];

            if (outChilds[nodeIndex] == 0)
            {
                _ticksTotal += ticks;
            }

            outTimes[nodeIndex] = ticks * _ticksToMilliseconds;
            
            _stackLast--;
        }
    }
}