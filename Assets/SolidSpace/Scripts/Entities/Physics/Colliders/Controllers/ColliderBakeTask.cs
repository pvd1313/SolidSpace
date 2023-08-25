using System;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Utilities;
using SolidSpace.Entities.World;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Colliders
{
    public struct ColliderBakeTask<T> where T : struct, IColliderBakeBehaviour
    {
        public ProfilingHandle profiler;
        public IEntityManager entityManager;
        public NativeArray<ArchetypeChunk> archetypeChunks;

        public BakedColliders Bake(ref T behaviour)
        {
            profiler.BeginSample("Compute offsets");
            var chunkOffsets = EntityQueryForJobUtil.ComputeOffsets(archetypeChunks);
            profiler.EndSample("Compute offsets");

            var colliderCount = chunkOffsets.entityCount;
            if (colliderCount > ushort.MaxValue)
            {
                throw new InvalidOperationException($"Collider count exceeded max value ({ushort.MaxValue})");
            }
            
            profiler.BeginSample("Collect data");
            behaviour.OnInitialize(chunkOffsets.entityCount);
            var dataCollectJob = new ColliderDataCollectJob<T>
            {
                behaviour = behaviour,
                inArchetypeChunks = archetypeChunks,
                inWriteOffsets = chunkOffsets.chunkOffsets,
                positionHandle = entityManager.GetComponentTypeHandle<PositionComponent>(true),
                rotationHandle = entityManager.GetComponentTypeHandle<RotationComponent>(true),
                rectSizeHandle = entityManager.GetComponentTypeHandle<RectSizeComponent>(true),
                outShapes = NativeMemory.CreateTempArray<ColliderShape>(colliderCount),
                outBounds = NativeMemory.CreateTempArray<FloatBounds>(colliderCount),
            };
            dataCollectJob.Schedule(chunkOffsets.chunkCount, 1).Complete();
            profiler.EndSample("Collect data");
            
            profiler.BeginSample("Construct grid");
            var worldGrid = ColliderUtil.ComputeGrid(dataCollectJob.outBounds, colliderCount, profiler);
            profiler.EndSample("Construct grid");
            
            profiler.BeginSample("Allocate cells");
            var worldCellTotal = worldGrid.size.x * worldGrid.size.y;
            var worldCells = NativeMemory.CreateTempArray<ColliderListPointer>(worldCellTotal);
            new FillNativeArrayJob<ColliderListPointer>
            {
                inItemPerJob = 128,
                inValue = default,
                inTotalItem = worldCellTotal,
                outNativeArray = worldCells
            }.Schedule((int) Math.Ceiling(worldCellTotal / 128f), 1).Complete();
            profiler.EndSample("Allocate cells");
            
            profiler.BeginSample("Bake colliders");
            var jobCount = (int) Math.Ceiling(colliderCount / 128f);
            var bakingJob = new ChunkCollidersJob
            {
                inColliderBounds = dataCollectJob.outBounds,
                inWorldGrid = worldGrid,
                inColliderPerJob = 128,
                inColliderTotalCount = colliderCount,
                outColliders = NativeMemory.CreateTempArray<ChunkedCollider>(colliderCount * 4),
                outColliderCounts = NativeMemory.CreateTempArray<int>(jobCount)
            };
            bakingJob.Schedule(jobCount, 1).Complete();
            profiler.EndSample("Bake colliders");
            
            profiler.BeginSample("Lists capacity");
            new ChunkListsCapacityJob
            {
                inColliderBatchCapacity = 128 * 4,
                inColliders = bakingJob.outColliders,
                inColliderCounts = bakingJob.outColliderCounts,
                inOutLists = worldCells
            }.Run();
            profiler.EndSample("Lists capacity");
            
            profiler.BeginSample("Lists offsets");
            new ChunkListsOffsetJob
            {
                inListCount = worldCellTotal,
                inOutLists = worldCells
            }.Run();
            profiler.EndSample("Lists offsets");

            profiler.BeginSample("Lists fill");
            var listsFillJob = new ChunkListsFillJob
            {
                inColliderBatchCapacity = 128 * 4,
                inColliders = bakingJob.outColliders,
                inColliderCounts = bakingJob.outColliderCounts,
                inOutLists = worldCells,
                outColliders = NativeMemory.CreateTempArray<ushort>(colliderCount * 4)
            };
            listsFillJob.Run();
            profiler.EndSample("Lists fill");

            profiler.BeginSample("Disposal");
            chunkOffsets.chunkOffsets.Dispose();
            bakingJob.outColliders.Dispose();
            bakingJob.outColliderCounts.Dispose();
            profiler.EndSample("Disposal");

            return new BakedColliders
            {
                shapes = dataCollectJob.outShapes,
                bounds = dataCollectJob.outBounds,
                grid = worldGrid,
                cells = worldCells,
                indices = listsFillJob.outColliders
            };
        }
    }
}