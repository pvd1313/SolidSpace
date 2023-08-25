using System.Runtime.CompilerServices;

namespace SolidSpace.Mathematics
{
    public static class BinaryMath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfTwo(int a)
        {
            return (a != 0) && ((a & (a - 1)) == 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPowerOfFour(int a)
        {
            if (!IsPowerOfTwo(a))
            {
                return false;
            }

            return (a & 0x55555555) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFirstBitIndex64(ulong value)
        {
            if (value == 0)
            {
                return -1;
            }

            return UnsafeGetFirstBitIndex64(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFirstBitIndex32(uint value)
        {
            if (value == 0)
            {
                return -1;
            }

            return UnsafeGetFirstBitIndex32(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFirstBitIndex16(ushort value)
        {
            if (value == 0)
            {
                return -1;
            }

            return UnsafeGetFirstBitIndex16(value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFirstBitIndex8(byte value)
        {
            if (value == 0)
            {
                return -1;
            }

            return UnsafeGetFirstBitIndex8(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UnsafeGetFirstBitIndex64(ulong value)
        {
            if ((value & 0xFFFFFFFFul) != 0)
            {
                return UnsafeGetFirstBitIndex32((uint) value);
            }

            return 32 + UnsafeGetFirstBitIndex32((uint) (value >> 32));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UnsafeGetFirstBitIndex32(uint value)
        {
            if ((value & 0xFFFF) != 0)
            {
                return UnsafeGetFirstBitIndex16((ushort) value);
            }

            return 16 + UnsafeGetFirstBitIndex16((ushort) (value >> 16));
        }

        private static int UnsafeGetFirstBitIndex16(ushort value)
        {
            if ((value & 0xFF) != 0)
            {
                return UnsafeGetFirstBitIndex8((byte) value);
            }
            
            return 8 + UnsafeGetFirstBitIndex8((byte) (value >> 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UnsafeGetFirstBitIndex8(byte value)
        {
            if ((value & 0xF) != 0)
            {
                return UnsafeGetFirstBitIndex4(value);
            }

            return 4 + UnsafeGetFirstBitIndex4((byte) (value >> 4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int UnsafeGetFirstBitIndex4(byte value4Bit)
        {
            if ((value4Bit & 3) != 0)
            {
                if ((value4Bit & 1) != 0)
                {
                    return 0;
                }

                return 1;
            }
            
            if ((value4Bit & 4) != 0)
            {
                return 2;
            }

            return 3;
        }
    }
}