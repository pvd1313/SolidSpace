using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;
using Unity.Mathematics;

namespace SolidSpace.Entities.Splitting
{
    public struct ByteBounds
    {
        public byte2 min;
        public byte2 max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int2 GetSize()
        {
            return new int2
            {
                x = max.x - min.x + 1,
                y = max.y - min.y + 1
            };
        }
    }
}