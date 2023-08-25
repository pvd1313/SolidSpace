using System.Collections.Generic;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Despawn;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Prefabs;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Splitting
{
    public class SplittingController : ISplittingCommandSystem, IInitializable, IUpdatable
    {
        private readonly IEntityDestructionSystem _destructionSystem;
        private readonly IPrefabSystem _prefabSystem;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly IProfilingManager _profilingManager;
        
        private ProfilingHandle _profiler;
        private List<SplittingEntityData> _splittingQueue;
        private HashSet<Entity> _entities;

        public SplittingController(IProfilingManager profilingManager,
                                   IHealthAtlasSystem healthSystem,
                                   IEntityDestructionSystem destructionSystem,
                                   IPrefabSystem prefabSystem)
        {
            _profilingManager = profilingManager;
            _healthSystem = healthSystem;
            _destructionSystem = destructionSystem;
            _prefabSystem = prefabSystem;
        }

        public void OnInitialize()
        {
            _splittingQueue = new List<SplittingEntityData>();
            _entities = new HashSet<Entity>();
            _profiler = _profilingManager.GetHandle(this);
        }

        public void OnFinalize()
        {
        }

        public void ScheduleSplittingCheck(SplittingEntityData entityData)
        {
            if (_entities.Add(entityData.entity))
            {
                _splittingQueue.Add(entityData);
            }
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Collect entity data");
            var entityCount = _splittingQueue.Count;
            var seedMaskSize = 0;
            var shapeReading = NativeMemory.CreateTempArray<ShapeReadingData>(entityCount);
            var entityIndex = 0;

            foreach (var entityData in _splittingQueue)
            {
                shapeReading[entityIndex] = new ShapeReadingData
                {
                    entity = entityData.entity,
                    seedMaskOffset = seedMaskSize,
                    healthAtlasOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks, entityData.health),
                    entitySize = entityData.size,
                };

                seedMaskSize += entityData.size.x * entityData.size.y;

                entityIndex++;
            }

            _entities.Clear();
            _splittingQueue.Clear();
            _profiler.EndSample("Collect entity data");

            _profiler.BeginSample("Allocate arrays");
            var connections = NativeMemory.CreateTempArray<byte2>(256 * entityCount);
            var seedResults = NativeMemory.CreateTempArray<ShapeSeedJobResult>(entityCount);
            var bounds = NativeMemory.CreateTempArray<ByteBounds>(256 * entityCount);
            var seedMask = NativeMemory.CreateTempArray<byte>(seedMaskSize);
            var shapeCounts = NativeMemory.CreateTempArray<int>(entityCount);
            var shapeRootSeeds = NativeMemory.CreateTempArray<byte>(256 * entityCount);
            var jobHandles = NativeMemory.CreateTempArray<JobHandle>(entityCount);
            _profiler.EndSample("Allocate arrays");

            _profiler.BeginSample("Schedule jobs");
            for (var i = 0; i < entityCount; i++)
            {
                var entity = shapeReading[i];
                var frameLength = HealthUtil.GetRequiredByteCount(entity.entitySize);
                var maskSize = entity.entitySize.x * entity.entitySize.y;

                var seedJob = new ShapeSeedJob
                {
                    inFrameBits = _healthSystem.Data.Slice(entity.healthAtlasOffset, frameLength),
                    inFrameSize = entity.entitySize,
                    outConnections = connections.Slice(i * 256, 256),
                    outResult = seedResults.Slice(i, 1),
                    outSeedBounds = bounds.Slice(i * 256, 256),
                    outSeedMask = seedMask.Slice(entity.seedMaskOffset, maskSize)
                };

                var readJob = new ShapeReadJob
                {
                    inOutConnections = seedJob.outConnections,
                    inOutBounds = seedJob.outSeedBounds,
                    inSeedJobResult = seedJob.outResult,
                    outShapeCount = shapeCounts.Slice(i, 1),
                    outShapeRootSeeds = shapeRootSeeds.Slice(i * 256, 256)
                };

                jobHandles[i] = readJob.Schedule(seedJob.Schedule());
            }

            _profiler.EndSample("Schedule jobs");

            _profiler.BeginSample("Complete shape seed");
            JobHandle.CombineDependencies(jobHandles).Complete();
            _profiler.EndSample("Complete shape seed");

            _profiler.BeginSample("Schedule health building & replication");
            var estimatedChildCount = 0;
            for (var i = 0; i < entityCount; i++)
            {
                estimatedChildCount += shapeCounts[i];
            }

            jobHandles.Dispose();
            jobHandles = NativeMemory.CreateTempArray<JobHandle>(estimatedChildCount);
            var handleCount = 0;
            
            for (var parentId = 0; parentId < entityCount; parentId++)
            {
                var seedResult = seedResults[parentId];
                if (seedResult.code != EShapeSeedResult.Success)
                {
                    Debug.LogError($"Seeding job ended with result {seedResult.code}.");
                }

                var entity = shapeReading[parentId];
                var shapeCount = shapeCounts[parentId];
                if (shapeCount == 0)
                {
                    _destructionSystem.ScheduleDestroy(entity.entity);
                    continue;
                }

                if (shapeCount == 1)
                {
                    var childSize = bounds[parentId * 256].GetSize();
                    if ((entity.entitySize.x == childSize.x) && (entity.entitySize.y == childSize.y))
                    {
                        continue;
                    }
                }
                
                _destructionSystem.ScheduleDestroy(entity.entity);
                
                var parentData = shapeReading[parentId];

                for (var childId = 0; childId < shapeCount; childId++)
                {
                    var childBounds = bounds[parentId * 256 + childId];
                    var childSize = childBounds.GetSize();
                    var childHealth = _healthSystem.Allocate(childSize);

                    jobHandles[handleCount++] = new BlitShapeHealthFromMaskJob
                    {
                        inConnections = connections.Slice(parentId * 256, seedResult.connectionCount),
                        
                        inSourceOffset = new int2(childBounds.min.x, childBounds.min.y),
                        inBlitSize = childSize,
                        inSourceSize = parentData.entitySize,
                        inBlitShapeSeed = shapeRootSeeds[parentId * 256 + childId],
                        inTargetOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks, childHealth),

                        inSourceSeedMask = seedMask.Slice(
                            parentData.seedMaskOffset, 
                            parentData.entitySize.x * parentData.entitySize.y),
                        
                        outTargetHealth = _healthSystem.Data

                    }.Schedule();

                    _prefabSystem.ScheduleReplication(new PrefabReplicationData
                    {
                        parent = parentData.entity,
                        childHealth = childHealth,
                        childBounds = childBounds
                    });
                }
            }
            _profiler.EndSample("Schedule health building & replication");

            _profiler.BeginSample("Complete health jobs");
            JobHandle.CombineDependencies(jobHandles.Slice(0, handleCount)).Complete();
            _profiler.EndSample("Complete health jobs");
            
            _profiler.BeginSample("Disposal");
            shapeReading.Dispose();
            connections.Dispose();
            seedResults.Dispose();
            bounds.Dispose();
            seedMask.Dispose();
            shapeCounts.Dispose();
            shapeRootSeeds.Dispose();
            jobHandles.Dispose();
            _profiler.EndSample("Disposal");
        }
    }
}