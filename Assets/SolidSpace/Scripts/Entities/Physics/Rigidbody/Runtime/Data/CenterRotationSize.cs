using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Rigidbody
{
    public struct CenterRotationSize
    {
        public float2 center;
        public float rotation;
        public float2 size;

        public CenterRotationSize(float2 center, float rotation, float2 size)
        {
            this.center = center;
            this.rotation = rotation;
            this.size = size;
        }
    }
}