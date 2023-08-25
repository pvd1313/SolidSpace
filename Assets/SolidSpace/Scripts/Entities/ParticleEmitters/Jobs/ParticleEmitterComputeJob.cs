using SolidSpace.Entities.Components;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.ParticleEmitters
{
    [BurstCompile]
    internal struct ParticleEmitterComputeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public NativeArray<int> inWriteOffsets;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<RandomValueComponent> randomHandle;
        [ReadOnly] public ComponentTypeHandle<ParticleEmitterComponent> emitterHandle;
        [ReadOnly] public ComponentTypeHandle<RotationComponent> rotationHandle;
        [ReadOnly] public float inTime;

        public ComponentTypeHandle<RepeatTimerComponent> timerHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ParticleEmitterData> outParticles;
        [WriteOnly] public NativeArray<int> outParticleCounts;

        public void Execute(int chunkIndex)
        {
            var chunk = inChunks[chunkIndex];
            var writeOffset = inWriteOffsets[chunkIndex];
            var entityCount = chunk.Count;
            var emitters = chunk.GetNativeArray(emitterHandle);
            var positions = chunk.GetNativeArray(positionHandle);
            var timers = chunk.GetNativeArray(timerHandle);
            var randoms = chunk.GetNativeArray(randomHandle);

            NativeArray<RotationComponent> rotations = default;
            var hasRotation = chunk.Has(rotationHandle);
            if (hasRotation)
            {
                rotations = chunk.GetNativeArray(rotationHandle);
            }

            ParticleEmitterData emitterData;
            emitterData.despawnTime = inTime + 5;
            
            var emitCount = 0;
            for (var i = 0; i < entityCount; i++)
            {
                var timer = timers[i];
                if (timer.counter == 0)
                {
                    continue;
                }

                var forwardAngle = hasRotation ? rotations[i].value : 0;

                timer.counter = 0;
                timers[i] = timer;

                var randomValue = randoms[i].value;
                var emitter = emitters[i];
                var particleAngle = forwardAngle + emitter.spreadAngle * (0.5f - randomValue);
                emitterData.velocity = FloatMath.Rotate(emitter.particleVelocity, particleAngle);
                emitterData.position = positions[i].value;

                outParticles[writeOffset + emitCount] = emitterData;
                emitCount++;
            }

            outParticleCounts[chunkIndex] = emitCount;
        }
    }
}