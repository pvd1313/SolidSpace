using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.EntitySearch
{
    internal class EntitySearchSystem : IInitializable, IUpdatable, IEntitySearchSystem
    {
        private const int BufferSize = 512;
        
        public EntitySearchResult Result { get; private set; }
        
        private readonly IEntityManager _entityManager;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _query;
        private ProfilingHandle _profiler;
        private float2 _searchPosition;
        private float _searchRadius;
        private bool _enabled;
        private NativeArray<Entity> _entitiesInRadius;
        private NativeArray<float2> _pointsInRadius;

        public EntitySearchSystem(IEntityManager entityManager, IProfilingManager profilingManager)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
        }
        
        public void OnInitialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _entitiesInRadius = NativeMemory.CreatePermArray<Entity>(BufferSize);
            _pointsInRadius = NativeMemory.CreatePermArray<float2>(BufferSize);
        }
        
        public void SetSearchPosition(float2 position)
        {
            _searchPosition = position;
        }

        public void SetSearchRadius(float radius)
        {
            _searchRadius = radius;
        }
        
        public void SetQuery(EntityQueryDesc queryDesc)
        {
            _query = _entityManager.CreateEntityQuery(queryDesc);
        }
        
        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
        }
        
        public void OnUpdate()
        {
            Result = new EntitySearchResult
            {
                isValid = false
            };
            
            if (!_enabled)
            {
                return;
            }

            var archetypeChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            if (archetypeChunks.Length == 0)
            {
                archetypeChunks.Dispose();
                return;
            }

            _profiler.BeginSample("Offsets");
            var archetypeChunkCount = archetypeChunks.Length;
            var radiusOffsets = NativeMemory.CreateTempArray<int>(archetypeChunkCount);
            var entityCount = 0;
            for (var i = 0; i < archetypeChunkCount; i++)
            {
                var chunk = archetypeChunks[i];
                radiusOffsets[i] = entityCount;
                entityCount += chunk.Count;
            }
            _profiler.EndSample("Offsets");
            
            _profiler.BeginSample("Job");
            var radiusCounts = NativeMemory.CreateTempArray<int>(archetypeChunkCount);
            var nearestPositions = NativeMemory.CreateTempArray<float2>(archetypeChunks.Length);
            var nearestEntities = NativeMemory.CreateTempArray<Entity>(archetypeChunks.Length);
            var rule = new ArrayMaintenanceData
            {
                itemPerAllocation = BufferSize,
                requiredCapacity = entityCount,
                copyOnResize = false
            };
            NativeMemory.MaintainPersistentArrayLength(ref _entitiesInRadius, rule);
            NativeMemory.MaintainPersistentArrayLength(ref _pointsInRadius, rule);
            new SearchEntitiesJob
            {
                inChunks = archetypeChunks,
                inSearchRadius = _searchRadius,
                inRadiusWriteOffsets = radiusOffsets,
                inSearchPoint = _searchPosition,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                entityHandle = _entityManager.GetEntityTypeHandle(),
                outNearestPositions = nearestPositions,
                outNearestEntities = nearestEntities,
                outEntitiesInRadius = _entitiesInRadius,
                outInRadiusCounts = radiusCounts,
                outPointsInRadius = _pointsInRadius
            }.Schedule(archetypeChunks.Length, 4).Complete();
            _profiler.EndSample("Job");
            
            _profiler.BeginSample("Get nearest");
            var minDistance = float.MaxValue;
            var minPosition = float2.zero;
            Entity minEntity = default;
            for (var i = 0; i < archetypeChunks.Length; i++)
            {
                var position = nearestPositions[i];
                var distance = FloatMath.Distance(nearestPositions[i], _searchPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minPosition = position;
                    minEntity = nearestEntities[i];
                }
            }
            _profiler.EndSample("Get nearest");

            _profiler.BeginSample("Collect inRadius");
            var inRadiusTotalCount = NativeMemory.CreateTempJobReference<int>();
            new DataCollectJobWithOffsets<Entity, float2>
            {
                inDataCount = radiusCounts.Length,
                inCounts = radiusCounts,
                inOffsets = radiusOffsets,
                inOutData0 = _entitiesInRadius,
                inOutData1 = _pointsInRadius,
                outCount = inRadiusTotalCount
            }.Run();
            _profiler.EndSample("Collect inRadius");

            Result = new EntitySearchResult
            {
                isValid = true,
                nearestEntity = minEntity,
                nearestPosition = minPosition,
                inRadiusCount = inRadiusTotalCount.Value,
                inRadiusEntities = new NativeSlice<Entity>(_entitiesInRadius, 0, inRadiusTotalCount.Value),
                inRadiusPositions = new NativeSlice<float2>(_pointsInRadius,0, inRadiusTotalCount.Value)
            };
            
            _profiler.BeginSample("Dispose arrays");
            nearestPositions.Dispose();
            nearestEntities.Dispose();
            archetypeChunks.Dispose();
            radiusCounts.Dispose();
            inRadiusTotalCount.Dispose();
            radiusOffsets.Dispose();
            _profiler.EndSample("Dispose arrays");
        }

        public void OnFinalize()
        {
            _entitiesInRadius.Dispose();
            _pointsInRadius.Dispose();
        }
    }
}