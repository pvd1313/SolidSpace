using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Despawn;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Entities.Physics.Raycast;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.Splitting;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Gizmos;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Bullets
{
    public class BulletComputeSystem : IInitializable, IUpdatable
    {
        private readonly IProfilingManager _profilingManager;
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _worldTime;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly IGizmosManager _gizmosManager;
        private readonly IEntityDestructionSystem _destructionSystem;
        private readonly ISplittingCommandSystem _splittingSystem;
        private readonly ISpriteFrameSystem _frameSystem;

        private ProfilingHandle _profiler;
        private NativeArray<Entity> _entitiesToDestroy;
        private GizmosHandle _gridGizmos;
        private GizmosHandle _colliderGizmos;
        private EntityQuery _colliderQuery;
        private EntityQuery _bulletQuery;
        private Mask256 _pixelConnectionMask;
        private Mask256 _aloneBorderPixelMask;

        public BulletComputeSystem(
            IEntityWorldTime worldTime, 
            IProfilingManager profilingManager, 
            IEntityManager entityManager,
            IHealthAtlasSystem healthSystem, 
            IGizmosManager gizmosManager,
            IEntityDestructionSystem destructionSystem,
            ISplittingCommandSystem splittingSystem,
            ISpriteFrameSystem frameSystem)
        {
            _profilingManager = profilingManager;
            _entityManager = entityManager;
            _worldTime = worldTime;
            _healthSystem = healthSystem;
            _gizmosManager = gizmosManager;
            _destructionSystem = destructionSystem;
            _splittingSystem = splittingSystem;
            _frameSystem = frameSystem;
        }
        
        public void OnInitialize()
        {
            _gridGizmos = _gizmosManager.GetHandle(this, "Grid", Color.gray);
            _colliderGizmos = _gizmosManager.GetHandle(this, "Collider", Color.green);
            _profiler = _profilingManager.GetHandle(this);
            _colliderQuery = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RectSizeComponent),
                typeof(SpriteRenderComponent),
                typeof(HealthComponent),
            });
            _bulletQuery = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(BulletComponent)
            });
            _entitiesToDestroy = NativeMemory.CreateTempArray<Entity>(0);
            _pixelConnectionMask = SplittingUtil.Bake4NeighbourPixelConnectionMask();
            _aloneBorderPixelMask = SplittingUtil.BakeAloneBorderPixelMask();
        }
        
        public void OnUpdate()
        {
            var bakeBehaviour = new BulletColliderBakeBehaviour
            {
                healthHandle = _entityManager.GetComponentTypeHandle<HealthComponent>(true),
                spriteHandle = _entityManager.GetComponentTypeHandle<SpriteRenderComponent>(true),
                entityHandle = _entityManager.GetEntityTypeHandle(),
            };
            
            _profiler.BeginSample("Query colliders");
            var colliderArchetypeChunks = _colliderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query colliders");
            
            _profiler.BeginSample("Bake colliders");
            var colliders = new ColliderBakeTask<BulletColliderBakeBehaviour>
            {
                profiler = _profiler,
                entityManager = _entityManager,
                archetypeChunks = colliderArchetypeChunks
            }.Bake(ref bakeBehaviour);
            _profiler.EndSample("Bake colliders");

            _profiler.BeginSample("Query bullets");
            var bulletArchetypeChunks = _bulletQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query bullets");

            var raycastBehaviour = new BulletRaycastBehaviour
            {
                inColliders = colliders,
                inColliderHealths = bakeBehaviour.outHealthComponents,
                inColliderSprites = bakeBehaviour.outSpriteComponents,
                inColliderEntities = bakeBehaviour.outEntities,
                inDeltaTime = _worldTime.DeltaTime,
                entityHandle = _entityManager.GetEntityTypeHandle(),
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                velocityHandle = _entityManager.GetComponentTypeHandle<VelocityComponent>(true),
                inHealthAtlas = _healthSystem.Data,
                inHealthChunks = _healthSystem.Chunks,
            };

            _profiler.BeginSample("Raycast");
            new RaycastTask<BulletRaycastBehaviour>
            {
                profiler = _profiler,
                colliders = colliders,
                archetypeChunks = bulletArchetypeChunks,
            }.Raycast(ref raycastBehaviour);
            _profiler.EndSample("Raycast");

            _profiler.BeginSample("Apply damage");
            var hitCount = raycastBehaviour.outCount.Value;
            var hits = raycastBehaviour.outHits;
            var healthAtlas = _healthSystem.Data;
            
            _entitiesToDestroy.Dispose();
            _entitiesToDestroy = NativeMemory.CreateTempArray<Entity>(hitCount);

            for (var i = 0; i < hitCount; i++)
            {
                var hit = hits[i];
                _entitiesToDestroy[i] = hit.bulletEntity;

                var frameOffset = AtlasMath.ComputeOffset(_frameSystem.Chunks, hit.colliderFrame);
                frameOffset += new int3(hit.hitPixel.x, hit.hitPixel.y, 0);
                _frameSystem.SetFrame(frameOffset, false);

                var colliderSize = new int2((int) hit.colliderSize.x, (int) hit.colliderSize.y);
                var healthOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks, hit.colliderHealth);
                var healthAtlasSlice = healthAtlas.Slice(healthOffset);
                HealthUtil.ClearBit(healthAtlasSlice, colliderSize, hit.hitPixel);

                var neighbourPixels = SplittingUtil.ReadNeighbourPixels(healthAtlasSlice, colliderSize, hit.hitPixel);
                
                if (!_pixelConnectionMask.HasBit(neighbourPixels) || 
                    IsAloneBorderPixel(hit.hitPixel, colliderSize, neighbourPixels))
                {
                    _splittingSystem.ScheduleSplittingCheck(new SplittingEntityData
                    {
                        entity = hit.colliderEntity,
                        health = hit.colliderHealth,
                        size = colliderSize
                    });   
                }
            }
            _destructionSystem.ScheduleDestroy(_entitiesToDestroy.Slice(0, hitCount));
            _profiler.EndSample("Apply damage");
            
            _profiler.BeginSample("Gizmos");
            ColliderGizmosUtil.DrawGrid(_gridGizmos, colliders.grid);
            ColliderGizmosUtil.DrawColliders(_colliderGizmos, colliders);
            _profiler.EndSample("Gizmos");
            
            _profiler.BeginSample("Dispose arrays");
            colliderArchetypeChunks.Dispose();
            bulletArchetypeChunks.Dispose();
            bakeBehaviour.Dispose();
            raycastBehaviour.Dispose();
            colliders.Dispose();
            _profiler.EndSample("Dispose arrays");
        }
        
        private bool IsAloneBorderPixel(int2 pixelPosition, int2 frameSize, byte neighbourPixels)
        {
            if (pixelPosition.x == 0 || 
                pixelPosition.x == frameSize.x - 1 || 
                pixelPosition.y == 0 || 
                pixelPosition.y == frameSize.y - 1)
            {
                return _aloneBorderPixelMask.HasBit(neighbourPixels);
            }

            return false;
        }

        public void OnFinalize()
        {
            _entitiesToDestroy.Dispose();
        }
    }
}