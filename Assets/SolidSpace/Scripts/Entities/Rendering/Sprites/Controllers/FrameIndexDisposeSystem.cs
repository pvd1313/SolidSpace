using SolidSpace.Entities.Components;
using SolidSpace.Entities.Despawn;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public class FrameIndexDisposeSystem : IInitializable, IEntityDestructionHandler
    {
        private readonly IEntityManager _entityManager;
        private readonly IProfilingManager _profilingManager;
        private readonly ISpriteFrameSystem _frameSystem;

        private EntityQuery _query;

        public FrameIndexDisposeSystem(IEntityManager entityManager,
                                           IProfilingManager profilingManager,
                                           ISpriteFrameSystem frameSystem)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
            _frameSystem = frameSystem;
        }

        public void OnInitialize()
        {
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(DestroyedComponent),
                typeof(SpriteRenderComponent)
            });
        }
        
        public void OnFinalize()
        {
            
        }

        public void OnBeforeEntitiesDestroyed()
        {
            var sprites = _query.ToComponentDataArray<SpriteRenderComponent>(Allocator.TempJob);

            foreach (var sprite in sprites)
            {
                _frameSystem.Release(sprite.frameIndex);
            }

            sprites.Dispose();
        }
    }
}