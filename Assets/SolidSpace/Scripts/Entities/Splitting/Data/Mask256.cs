using System;
using System.Runtime.CompilerServices;

namespace SolidSpace.Entities.Splitting
{
    public struct Mask256
    {
        public long v0;
        public long v1;
        public long v2;
        public long v3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasBit(byte index)
        {
            if (index < 128)
            {
                if (index < 64)
                {
                    return (v0 & (1L << index)) != 0;
                }

                return (v1 & (1L << (index - 64))) != 0;
            }

            if (index < 192)
            {
                return (v2 & (1L << (index - 128))) != 0;
            }
            
            return (v3 & (1L << (index - 192))) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBitTrue(byte index)
        {
            if (index < 128)
            {
                if (index < 64)
                {
                    v0 |= 1L << index;
                    return;
                }

                v1 |= 1L << (index - 64);
                return;
            }

            if (index < 192)
            {
                v2 |= 1L << (index - 128);
                return;
            }

            v3 |= 1L << (index - 192);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Mask256 operator &(Mask256 a, Mask256 b)
        {
            return new Mask256
            {
                v0 = a.v0 & b.v0,
                v1 = a.v1 & b.v1,
                v2 = a.v2 & b.v2,
                v3 = a.v3 & b.v3,
            };
        }

        public static Mask256 operator ^(Mask256 a, Mask256 b)
        {
            return new Mask256
            {
                v0 = a.v0 ^ b.v0,
                v1 = a.v1 ^ b.v1,
                v2 = a.v2 ^ b.v2,
                v3 = a.v3 ^ b.v3,
            };
        }

        public static Mask256 operator |(Mask256 a, Mask256 b)
        {
            return new Mask256
            {
                v0 = a.v0 | b.v0,
                v1 = a.v1 | b.v1,
                v2 = a.v2 | b.v2,
                v3 = a.v3 | b.v3
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyBit()
        {
            return (v0 | v1 | v2 | v3) != 0L;
        }
    }
}