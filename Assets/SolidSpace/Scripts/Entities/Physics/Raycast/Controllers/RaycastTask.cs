using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.Utilities;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Raycast
{
    public struct RaycastTask<T> where T : struct, IRaycastBehaviour
    {
        private const int HitStackSize = 8;

        public ProfilingHandle profiler;
        public BakedColliders colliders;
        public NativeArray<ArchetypeChunk> archetypeChunks;

        public void Raycast(ref T behaviour)
        {
            profiler.BeginSample("Chunk offsets");
            var chunkOffsets = EntityQueryForJobUtil.ComputeOffsets(archetypeChunks);
            profiler.EndSample("Chunk offsets");

            profiler.BeginSample("Raycast");
            behaviour.Initialize(chunkOffsets.entityCount);
            var raycastJob = new RaycastJob<T>
            {
                behaviour = behaviour,
                hitStackSize = HitStackSize,
                inArchetypeChunks = archetypeChunks,
                inColliders = colliders,
                inWriteOffsets = chunkOffsets.chunkOffsets,
                hitStack = NativeMemory.CreateTempArray<ushort>(chunkOffsets.chunkCount * HitStackSize),
                outCounts = NativeMemory.CreateTempArray<int>(chunkOffsets.chunkCount)
            };
            raycastJob.Schedule(chunkOffsets.chunkCount, 1).Complete();
            profiler.EndSample("Raycast");
            
            profiler.BeginSample("Collect results");
            behaviour.CollectResult(chunkOffsets.chunkCount, chunkOffsets.chunkOffsets, raycastJob.outCounts);
            profiler.EndSample("Collect results");

            profiler.BeginSample("Dispose arrays");
            raycastJob.hitStack.Dispose();
            raycastJob.outCounts.Dispose();
            chunkOffsets.chunkOffsets.Dispose();
            profiler.EndSample("Dispose arrays");
        }
    }
}