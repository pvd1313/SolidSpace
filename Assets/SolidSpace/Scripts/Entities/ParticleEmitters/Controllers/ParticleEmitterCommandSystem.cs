using SolidSpace.Debugging;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.ParticleEmitters
{
    internal class ParticleEmitterCommandSystem : IInitializable, IUpdatable
    {
        private readonly IEntityManager _entityManager;
        private readonly IParticleEmitterComputeSystem _computeSystem;
        
        private EntityArchetype _particleArchetype;

        public ParticleEmitterCommandSystem(IEntityManager entityManager, IParticleEmitterComputeSystem computeSystem)
        {
            _entityManager = entityManager;
            _computeSystem = computeSystem;
        }

        public void OnInitialize()
        {
            _particleArchetype = _entityManager.CreateArchetype(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent),
                typeof(TimeDespawnComponent),
                typeof(BulletComponent),
                typeof(PixelRenderComponent)
            });
        }

        public void OnUpdate()
        {
            var entityCount = _computeSystem.ParticleCount;
            var particles = _computeSystem.Particles;
            var entities = _entityManager.CreateEntity(_particleArchetype, entityCount, Allocator.Temp);
            
            for (var i = 0; i < entityCount; i++)
            {
                var entity = entities[i];
                var particle = particles[i];
                _entityManager.SetComponentData(entity, new PositionComponent
                {
                    value = particle.position
                });
                _entityManager.SetComponentData(entity, new VelocityComponent
                {
                    value = particle.velocity
                });
                _entityManager.SetComponentData(entity, new TimeDespawnComponent
                {
                    despawnTime = particle.despawnTime
                });
            }
            
            SpaceDebug.LogState("EmittedCount", entityCount);
        }

        public void OnFinalize()
        {
            
        }
    }
}