using SolidSpace.Mathematics;

namespace SolidSpace.Entities.Physics.Raycast
{
    public struct RayHit
    {
        public FloatRay ray;
        public int rayIndex;
        public int writeOffset;
        public ushort colliderIndex;
    }
}