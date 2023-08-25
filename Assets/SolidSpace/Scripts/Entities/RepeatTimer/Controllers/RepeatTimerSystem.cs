using SolidSpace.Debugging;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.RepeatTimer
{
    public class RepeatTimerSystem : IInitializable, IUpdatable
    {
        private readonly IEntityManager _entityManager;
        private readonly IEntityWorldTime _time;

        private EntityQuery _query;

        public RepeatTimerSystem(IEntityManager entityManager, IEntityWorldTime time)
        {
            _entityManager = entityManager;
            _time = time;
        }
        
        public void OnInitialize()
        {
            _query = _entityManager.CreateEntityQuery(typeof(RepeatTimerComponent));
        }

        public void OnUpdate()
        {
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            
            new RepeatTimerJob
            {
                chunks = chunks,
                deltaTime = _time.DeltaTime,
                timerHandle = _entityManager.GetComponentTypeHandle<RepeatTimerComponent>(false)
            }.Schedule(chunks.Length, 32).Complete();

            chunks.Dispose();
            
            SpaceDebug.LogState("deltaTime", _time.DeltaTime);
        }

        public void OnFinalize()
        {
            
        }
    }
}