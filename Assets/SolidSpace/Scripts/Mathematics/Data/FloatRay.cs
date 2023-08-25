using Unity.Mathematics;

namespace SolidSpace.Mathematics
{
    public struct FloatRay
    {
        public float2 pos0;
        public float2 pos1;

        public FloatRay(float2 pos0, float2 pos1)
        {
            this.pos0 = pos0;
            this.pos1 = pos1;
        }
    }
}