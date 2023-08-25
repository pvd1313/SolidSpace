using System.Runtime.CompilerServices;

namespace SolidSpace.Mathematics
{
    public static class IntMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MinMax(int a, int b, out int min, out int max)
        {
            if (a < b)
            {
                min = a;
                max = b;
            }
            else
            {
                min = b;
                max = a;
            }
        }
    }
}