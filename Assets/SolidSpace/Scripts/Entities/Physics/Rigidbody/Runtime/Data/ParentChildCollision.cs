using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Rigidbody
{
    public struct ParentChildCollision
    {
        public float2 parentHalfBounds;
        public float2 childCenter;
        public Point4 childPoints;
    }
}