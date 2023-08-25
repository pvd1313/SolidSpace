using Unity.Entities;

namespace SolidSpace.Entities.Components
{
    public struct ParticleEmitterComponent : IComponentData
    {
        public float particleVelocity;
        public float spreadAngle;
    }
}