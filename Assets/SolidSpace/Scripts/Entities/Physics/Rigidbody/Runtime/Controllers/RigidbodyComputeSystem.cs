using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Rigidbody
{
    public class RigidbodyComputeSystem : IUpdatable, IInitializable
    {
        private const int CollisionStackSize = 32;
        
        private readonly IProfilingManager _profilingManager;
        private readonly IEntityManager _entityManager;

        private ProfilingHandle _profiler;
        private EntityQuery _query;
        
        public RigidbodyComputeSystem(IProfilingManager profilingManager, IEntityManager entityManager)
        {
            _profilingManager = profilingManager;
            _entityManager = entityManager;
        }
        
        public void OnInitialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RectColliderComponent),
                typeof(RectSizeComponent),
                typeof(RigidbodyComponent)
            });
        }
        
        public void OnUpdate()
        {
            var bakeBehaviour = new RigidbodyColliderBakeBehaviour
            {
                rigidbodyHandle = _entityManager.GetComponentTypeHandle<RigidbodyComponent>(false) 
            };
            
            _profiler.BeginSample("Query chunks");
            var archetypeChunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query chunks");
            
            _profiler.BeginSample("Bake colliders");
            var colliders = new ColliderBakeTask<RigidbodyColliderBakeBehaviour>
            {
                archetypeChunks = archetypeChunks,
                entityManager = _entityManager,
                profiler = _profiler
            }.Bake(ref bakeBehaviour);
            _profiler.EndSample("Bake colliders");
            
            _profiler.BeginSample("Collision job");
            var collisionJob = new RigidbodyCollisionJob
            {
                inMotionMultiplier = 0.5f,
                rigidbodyHandle = _entityManager.GetComponentTypeHandle<RigidbodyComponent>(true),
                inArchetypeChunks = archetypeChunks,
                inColliders = colliders,
                hitStack = NativeMemory.CreateTempArray<ushort>(archetypeChunks.Length * CollisionStackSize),
                hitStackSize = CollisionStackSize,
                outMotion = NativeMemory.CreateTempArray<float2>(colliders.shapes.Length)
            };
            collisionJob.Schedule(archetypeChunks.Length, 1).Complete();
            _profiler.EndSample("Collision job");

            _profiler.BeginSample("Motion job");
            new RigidbodyMotionJob
            {
                inMotion = collisionJob.outMotion,
                inArchetypeChunks = archetypeChunks,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(false),
                rigidbodyHandle = _entityManager.GetComponentTypeHandle<RigidbodyComponent>(true),
            }.Schedule(archetypeChunks.Length, 1).Complete();
            _profiler.EndSample("Motion job");
            
            _profiler.BeginSample("Dispose arrays");
            archetypeChunks.Dispose();
            colliders.Dispose();
            collisionJob.hitStack.Dispose();
            collisionJob.outMotion.Dispose();
            _profiler.EndSample("Dispose arrays");
        }

        public void OnFinalize()
        {
            
        }
    }
}