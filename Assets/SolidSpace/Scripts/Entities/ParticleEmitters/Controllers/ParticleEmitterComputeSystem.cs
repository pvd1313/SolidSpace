using SolidSpace.Entities.Components;
using SolidSpace.Entities.Utilities;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.ParticleEmitters
{
    internal class ParticleEmitterComputeSystem : IInitializable, IUpdatable, IParticleEmitterComputeSystem
    {
        private const int BufferChunkSize = 128;

        public NativeArray<ParticleEmitterData> Particles => _particles;
        public int ParticleCount => _particleCount.Value;
        
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _time;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _query;
        private NativeArray<ParticleEmitterData> _particles;
        private NativeReference<int> _particleCount;
        private ProfilingHandle _profiler;

        public ParticleEmitterComputeSystem(IEntityManager entityManager, IEntityWorldTime time, IProfilingManager profilingManager)
        {
            _entityManager = entityManager;
            _time = time;
            _profilingManager = profilingManager;
        }

        public void OnInitialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(ParticleEmitterComponent),
                typeof(PositionComponent),
                typeof(RandomValueComponent),
                typeof(RepeatTimerComponent)
            });
            _particles = NativeMemory.CreatePermArray<ParticleEmitterData>(BufferChunkSize);
            _particleCount = NativeMemory.CreatePersistentReference(0);
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Query Chunks");
            var chunks = EntityQueryForJobUtil.QueryWithOffsets(_query);
            _profiler.EndSample("Query Chunks");

            NativeMemory.MaintainPersistentArrayLength(ref _particles, new ArrayMaintenanceData
            {
                requiredCapacity = chunks.entityCount,
                itemPerAllocation = BufferChunkSize,
                copyOnResize = false
            });

            _profiler.BeginSample("Compute & Collect");
            var computeJob = new ParticleEmitterComputeJob
            {
                inChunks = chunks.chunks,
                inWriteOffsets = chunks.chunkOffsets,
                timerHandle = _entityManager.GetComponentTypeHandle<RepeatTimerComponent>(false),
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                randomHandle = _entityManager.GetComponentTypeHandle<RandomValueComponent>(true),
                emitterHandle = _entityManager.GetComponentTypeHandle<ParticleEmitterComponent>(true),
                rotationHandle = _entityManager.GetComponentTypeHandle<RotationComponent>(true),
                inTime = (float) _time.ElapsedTime,
                outParticles = _particles, 
                outParticleCounts = NativeMemory.CreateTempArray<int>(chunks.chunkCount),
            };
            var computeHandle = computeJob.Schedule(chunks.chunkCount, 32);

            new DataCollectJobWithOffsets<ParticleEmitterData>
            {
                inDataCount = chunks.chunkCount,
                inCounts = computeJob.outParticleCounts,
                inOffsets = chunks.chunkOffsets,
                inOutData = _particles,
                outCount = _particleCount
            }.Schedule(computeHandle).Complete();
            _profiler.EndSample("Compute & Collect");

            _profiler.BeginSample("Disposal");
            chunks.Dispose();
            computeJob.outParticleCounts.Dispose();
            _profiler.EndSample("Disposal");
        }

        public void OnFinalize()
        {
            _particles.Dispose();
            _particleCount.Dispose();
        }
    }
}