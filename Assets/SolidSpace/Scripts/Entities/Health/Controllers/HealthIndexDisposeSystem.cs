using SolidSpace.Entities.Components;
using SolidSpace.Entities.Despawn;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Health
{
    public class HealthIndexDisposeSystem : IInitializable, IEntityDestructionHandler
    {
        private readonly IHealthAtlasSystem _healthAtlas;
        private readonly IEntityManager _entityManager;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _query;

        public HealthIndexDisposeSystem(IHealthAtlasSystem healthAtlas, IEntityManager entityManager)
        {
            _healthAtlas = healthAtlas;
            _entityManager = entityManager;
        }
        
        public void OnInitialize()
        {
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(DestroyedComponent),
                typeof(HealthComponent)
            });
        }
        
        public void OnFinalize() { }
        
        public void OnBeforeEntitiesDestroyed()
        {
            var healths = _query.ToComponentDataArray<HealthComponent>(Allocator.TempJob);

            foreach (var healthComponent in healths)
            {
                _healthAtlas.Release(healthComponent.index);
            }
            
            healths.Dispose();
        }
    }
}