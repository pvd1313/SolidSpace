using SolidSpace.Entities.Components;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace SolidSpace.Entities.Randomization
{
    internal class RandomValueSystem : IInitializable, IUpdatable
    {
        private const int BufferChunkSize = 1024;

        private readonly IEntityManager _entityManager;
        
        private NativeArray<float> _buffer;
        private int _index;
        private EntityQuery _query;

        public RandomValueSystem(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        public void OnInitialize()
        {
            _buffer = NativeMemory.CreatePermArray<float>(BufferChunkSize);
            for (var i = 0; i < BufferChunkSize; i++)
            {
                _buffer[i] = Random.value;
            }
            _query = _entityManager.CreateEntityQuery(typeof(RandomValueComponent));
        }

        public void OnUpdate()
        {
            var entityCount = _query.CalculateEntityCount();
            var requiredCapacity = Mathf.CeilToInt(entityCount / (float) BufferChunkSize) * BufferChunkSize;
            if (_buffer.Length < requiredCapacity)
            {
                var newBuffer = NativeMemory.CreatePermArray<float>(requiredCapacity);
                for (var i = 0; i < _buffer.Length; i++)
                {
                    newBuffer[i] = _buffer[i];
                }
                for (var i = _buffer.Length; i < newBuffer.Length; i++)
                {
                    newBuffer[i] = Random.value;
                }
                _buffer.Dispose();
                _buffer = newBuffer;
            }
            
            _index = (_index + 1) % _buffer.Length;
            _buffer[_index] = Random.value;
            
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            var job = new RandomValueJob
            {
                chunks = chunks,
                randomHandle = _entityManager.GetComponentTypeHandle<RandomValueComponent>(false),
                randomIndex = _index,
                randomValues = _buffer
            };
            var handle = job.Schedule(chunks.Length, 32);
            
            handle.Complete();

            chunks.Dispose();
        }

        public void OnFinalize()
        {
            _buffer.Dispose();
        }
    }
}