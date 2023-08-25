using SolidSpace.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Velocity
{
    [BurstCompile]
    internal struct VelocityJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> chunks;
        [ReadOnly] public ComponentTypeHandle<VelocityComponent> velocityHandle;
        [ReadOnly] public float deltaTime;
        
        public ComponentTypeHandle<PositionComponent> positionHandle;

        public void Execute(int chunkIndex)
        {
            var chunk = chunks[chunkIndex];
            var positions = chunk.GetNativeArray(positionHandle);
            var velocities = chunk.GetNativeArray(velocityHandle);
            var entityCount = chunk.Count;

            for (var i = 0; i < entityCount; i++)
            {
                var positionComponent = positions[i];
                var velocity = velocities[i].value;
                positionComponent.value += velocity * deltaTime;
                positions[i] = positionComponent;
            }
        }
    }
}