using Unity.Collections;

namespace SolidSpace.Entities.ParticleEmitters
{
    internal interface IParticleEmitterComputeSystem
    {
        NativeArray<ParticleEmitterData> Particles { get; }
        int ParticleCount { get; }
    }
}