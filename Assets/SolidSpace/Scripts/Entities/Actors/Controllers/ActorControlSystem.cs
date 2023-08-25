using SolidSpace.Entities.Components;
using SolidSpace.Entities.Utilities;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Gizmos;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Actors
{
    public class ActorControlSystem : IInitializable, IUpdatable, IActorControlSystem
    {
        private readonly IEntityManager _entityManager;
        private readonly IProfilingManager _profilingManager;
        private readonly IEntityWorldTime _worldTime;
        private readonly IGizmosManager _gizmosManager;

        private EntityQuery _query;
        private float2 _targetPosition;
        private ProfilingHandle _profiler;
        private GizmosHandle _gizmos;

        public ActorControlSystem(IEntityManager entityManager, IProfilingManager profilingManager, 
            IEntityWorldTime worldTime, IGizmosManager gizmosManager)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
            _worldTime = worldTime;
            _gizmosManager = gizmosManager;
        }
        
        public void OnInitialize()
        {
            _gizmos = _gizmosManager.GetHandle(this, Color.magenta);
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(RotationComponent),
                typeof(ActorComponent)
            });
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Query chunks");
            var chunks = EntityQueryForJobUtil.QueryWithOffsets(_query);
            _profiler.EndSample("Query chunks");
            
            _profiler.BeginSample("Control Job");
            new ActorControlJob
            {
                inArchetypeChunks = chunks.chunks,
                inDeltaTime = _worldTime.DeltaTime,
                inSeekPosition = _targetPosition,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                velocityHandle = _entityManager.GetComponentTypeHandle<VelocityComponent>(false),
                actorHandle = _entityManager.GetComponentTypeHandle<ActorComponent>(true),
                rotationHandle = _entityManager.GetComponentTypeHandle<RotationComponent>(false)
                
            }.Schedule(chunks.chunkCount, 4).Complete();
            _profiler.EndSample("Control Job");

            _profiler.BeginSample("Gizmos filter");
            var filterJob = new FilterActivateActorsJob
            {
                inArchetypeChunks = chunks.chunks,
                inWriteOffsets = chunks.chunkOffsets,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                actorHandle = _entityManager.GetComponentTypeHandle<ActorComponent>(true),
                outPositions = NativeMemory.CreateTempArray<float2>(chunks.entityCount),
                outCounts = NativeMemory.CreateTempArray<int>(chunks.chunkCount)
            };
            filterJob.Schedule(chunks.chunkCount, 4).Complete();
            _profiler.EndSample("Gizmos filter");
            
            _profiler.BeginSample("Gizmos collect result");
            var positionCollectJob = new DataCollectJobWithOffsets<float2>
            {
                inDataCount = chunks.chunkCount,
                inCounts = filterJob.outCounts,
                inOutData = filterJob.outPositions,
                inOffsets = chunks.chunkOffsets,
                outCount = NativeMemory.CreateTempJobReference<int>()
            };
            positionCollectJob.Run();
            _profiler.EndSample("Gizmos collect result");
            
            _profiler.BeginSample("Draw gizmos");
            _gizmos.DrawScreenCircle(_targetPosition, 100f);
            var count = positionCollectJob.outCount.Value;
            for (var i = 0; i < count; i++)
            {
                _gizmos.DrawLine(positionCollectJob.inOutData[i], _targetPosition);
            }
            _profiler.EndSample("Draw gizmos");

            _profiler.BeginSample("Dispose arrays");
            chunks.Dispose();
            filterJob.outPositions.Dispose();
            filterJob.outCounts.Dispose();
            positionCollectJob.outCount.Dispose();
            _profiler.EndSample("Dispose arrays");
        }

        public void SetActorsTargetPosition(float2 position)
        {
            _targetPosition = position;
        }
        
        public void OnFinalize()
        {
            
        }
    }
}