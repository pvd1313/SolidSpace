using System.Collections.Generic;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Despawn
{
    internal class EntityDestructionSystem : IEntityDestructionSystem, IInitializable, IUpdatable
    {
        private readonly IEntityManager _entityManager;
        private readonly IProfilingManager _profilingManager;
        private readonly List<IEntityDestructionHandler> _destructionHandlers;

        private EntityQuery _query;
        private HashSet<Entity> _scheduledSingles;
        private List<NativeSlice<Entity>> _scheduledSlices;
        private List<EntityQuery> _scheduledQueries;
        private ProfilingHandle _profiler;

        public EntityDestructionSystem(IEntityManager entityManager,
                                       IProfilingManager profilingManager,
                                       List<IEntityDestructionHandler> destructionHandlers)
        {
            _entityManager = entityManager;
            _profilingManager = profilingManager;
            _destructionHandlers = destructionHandlers;
        }
        
        public void OnInitialize()
        {
            _scheduledSlices = new List<NativeSlice<Entity>>();
            _scheduledSingles = new HashSet<Entity>();
            _scheduledQueries = new List<EntityQuery>();
            _profiler = _profilingManager.GetHandle(this);
            _query = _entityManager.CreateEntityQuery(typeof(DestroyedComponent));
        }
        
        public void OnFinalize()
        {
        }

        public void ScheduleDestroy(NativeSlice<Entity> entities)
        {
            _scheduledSlices.Add(entities);
        }

        public void ScheduleDestroy(Entity entity)
        {
            _scheduledSingles.Add(entity);
        }

        public void ScheduleDestroy(EntityQuery query)
        {
            _scheduledQueries.Add(query);
        }

        public void OnUpdate()
        {
            while (_scheduledSingles.Count > 0 || _scheduledSlices.Count > 0 || _scheduledQueries.Count > 0)
            {
                _profiler.BeginSample("Count buffer entities");
                var entityCount = _scheduledSingles.Count;
                for (var i = 0; i < _scheduledSlices.Count; i++)
                {
                    entityCount += _scheduledSlices[i].Length;
                }
                _profiler.EndSample("Count buffer entities");

                _profiler.BeginSample("Blit entities buffer");
                var entities = NativeMemory.CreateTempArray<Entity>(entityCount);
                entityCount = 0;
                foreach (var entity in _scheduledSingles)
                {
                    entities[entityCount++] = entity;
                }

                foreach (var entitySlice in _scheduledSlices)
                {
                    for (var i = 0; i < entitySlice.Length; i++, entityCount++)
                    {
                        entities[entityCount] = entitySlice[i];
                    }
                }
                _profiler.EndSample("Blit entities buffer");
                
                _profiler.BeginSample("Tag entities buffer");
                _entityManager.AddComponent<DestroyedComponent>(entities);
                entities.Dispose();
                _profiler.EndSample("Tag entities buffer");

                _profiler.BeginSample("Tag queries");
                foreach (var query in _scheduledQueries)
                {
                    _entityManager.AddComponent<DestroyedComponent>(query);
                }
                _profiler.EndSample("Tag queries");
                
                _scheduledSlices.Clear();
                _scheduledSingles.Clear();
                _scheduledQueries.Clear();
                
                _profiler.BeginSample("Invoke handlers");
                foreach (var handler in _destructionHandlers)
                {
                    handler.OnBeforeEntitiesDestroyed();
                }
                _profiler.EndSample("Invoke handlers");
                
                _profiler.BeginSample("Destroy entities");
                _entityManager.DestroyEntity(_query);
                _profiler.EndSample("Destroy entities");
            }
        }
    }
}