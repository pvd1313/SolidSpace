using SolidSpace.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.RepeatTimer
{
    [BurstCompile]
    public struct RepeatTimerJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> chunks;
        [ReadOnly] public float deltaTime;
        
        public ComponentTypeHandle<RepeatTimerComponent> timerHandle;
        
        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var entityCount = chunk.Count;
            var timers = chunk.GetNativeArray(timerHandle);
            for (var i = 0; i < entityCount; i++)
            {
                var timer = timers[i];
                if (timer.counter == 8)
                {
                    continue;
                }

                timer.timer += deltaTime;
                if (timer.timer > timer.delay)
                {
                    timer.timer -= timer.delay;
                    timer.counter++;
                }

                timers[i] = timer;
            }
        }
    }
}