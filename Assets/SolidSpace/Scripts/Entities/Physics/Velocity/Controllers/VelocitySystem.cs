using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Velocity
{
    internal class VelocitySystem : IInitializable, IUpdatable
    {
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _time;

        private EntityQuery _query;

        public VelocitySystem(IEntityManager entityManager, IEntityWorldTime time)
        {
            _entityManager = entityManager;
            _time = time;
        }
        
        public void OnInitialize()
        {
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(VelocityComponent)
            });
        }

        public void OnUpdate()
        {
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            new VelocityJob
            {
                deltaTime = _time.DeltaTime,
                positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(false),
                velocityHandle = _entityManager.GetComponentTypeHandle<VelocityComponent>(true),
                chunks = chunks
            }.Schedule(chunks.Length, 32).Complete();

            chunks.Dispose();
        }

        public void OnFinalize()
        {
            
        }
    }
}