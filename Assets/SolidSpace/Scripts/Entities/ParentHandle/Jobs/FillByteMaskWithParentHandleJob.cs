using SolidSpace.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.ParentHandle
{
    [BurstCompile]
    public struct FillByteMaskWithParentHandleJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inArchetypeChunks;
        [ReadOnly] public ComponentTypeHandle<ParentComponent> parentHandle;

        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<byte> outMask;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inArchetypeChunks[chunkIndex];
            var entityCount = chunk.Count;
            var parentComponents = chunk.GetNativeArray(parentHandle);

            for (var i = 0; i < entityCount; i++)
            {
                var handle = parentComponents[i].handle;
                outMask[handle.index] = 1;
            }
        }
    }
}