using System;
using System.Runtime.CompilerServices;
using SolidSpace.Entities.Health;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;

namespace SolidSpace.Entities.Splitting
{
    public static class SplittingUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Mask256 BuildShapeMask(byte rootSeed, NativeSlice<byte2> connections)
        {
            Mask256 resultMask = default;
            
            resultMask.SetBitTrue((byte) (rootSeed - 1));
            for (var i = 0; i < connections.Length; i++)
            {
                var connection = connections[i];
                if (rootSeed == connection.x)
                {
                    resultMask.SetBitTrue((byte) (connection.y - 1));
                }
            }

            return resultMask;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadNeighbourPixels(NativeSlice<byte> frame, int2 spriteSize, int2 center)
        {
            var resultBits = 0;
            resultBits |= GetBit(frame, spriteSize, center + new int2(-1, -1)) << 0;
            resultBits |= GetBit(frame, spriteSize, center + new int2(+0, -1)) << 1;
            resultBits |= GetBit(frame, spriteSize, center + new int2(+1, -1)) << 2;
            resultBits |= GetBit(frame, spriteSize, center + new int2(-1, +0)) << 3;
            resultBits |= GetBit(frame, spriteSize, center + new int2(+1, +0)) << 4;
            resultBits |= GetBit(frame, spriteSize, center + new int2(-1, +1)) << 5;
            resultBits |= GetBit(frame, spriteSize, center + new int2(+0, +1)) << 6;
            resultBits |= GetBit(frame, spriteSize, center + new int2(+1, +1)) << 7;

            return (byte) resultBits;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetBit(NativeSlice<byte> frame, int2 spriteSize, int2 spritePoint)
        {
            if (spritePoint.x < 0 || spritePoint.x >= spriteSize.x)
            {
                return 0;
            }

            if (spritePoint.y < 0 || spritePoint.y >= spriteSize.y)
            {
                return 0;
            }

            return HealthUtil.HasBit(frame, spriteSize, spritePoint) ? 1 : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Mask256 BakeAloneBorderPixelMask()
        {
            var left = new int2(-1, 0);
            var up = new int2(0, 1);
            var down = new int2(0, -1);
            
            // ? - -
            // ? x -
            // ? - -
            return BakePattern(left, left + up, left + down);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Mask256 BakePattern(params int2[] variantPixels)
        {
            Mask256 resultMask = default;
            var variantTotalCount = 1 << variantPixels.Length;

            for (var rotation = 0; rotation < 4; rotation++)
            {
                for (var i = 0; i < variantTotalCount; i++)
                {
                    var state = 0;
                    for (var j = 0; j < variantPixels.Length; j++)
                    {
                        if ((i & (1 << j)) != 0)
                        {
                            state |= 1 << ToNormalizedIndex(Rotate90(variantPixels[j], rotation));
                        }
                    }
                    
                    resultMask.SetBitTrue((byte) state);
                }
            }

            return resultMask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int2 Rotate90(int2 direction, int rotation)
        {
            return rotation switch
            {
                0 => direction,
                1 => new int2(-direction.y, direction.x),
                2 => -direction,
                3 => new int2(direction.y, -direction.x),
                _ => throw new IndexOutOfRangeException()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ToNormalizedIndex(int2 direction)
        {
            var index = (direction.y + 1) * 3 + direction.x + 1;
            return index >= 5 ? index - 1 : index;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Mask256 Bake4NeighbourPixelConnectionMask()
        {
            return new Mask256
            {
                v0 = BakeMaskPartial(0),
                v1 = BakeMaskPartial(64),
                v2 = BakeMaskPartial(128),
                v3 = BakeMaskPartial(192),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long BakeMaskPartial(byte offset)
        {
            var resultMask = 0L;

            for (var i = 0; i < 64; i++)
            {
                if (CheckAll4NeighbourPixelsAreConnected((byte) (offset + i)))
                {
                    resultMask |= 1L << i;
                }
            }

            return resultMask;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckAll4NeighbourPixelsAreConnected(byte frame)
        {
            const byte p0 = 1;
            const byte p1 = 2;
            const byte p2 = 4;
            const byte p3 = 8;
            const byte p4 = 16;
            const byte p5 = 32;
            const byte p6 = 64;
            const byte p7 = 128;
            
            var frame0 = (byte) (frame & p0);
            var frame1 = (byte) (frame & p1);
            var frame2 = (byte) (frame & p2);
            var frame3 = (byte) (frame & p3);
            var frame4 = (byte) (frame & p4);
            var frame5 = (byte) (frame & p5);
            var frame6 = (byte) (frame & p6);
            var frame7 = (byte) (frame & p7);
            var fill = Math.Max(Math.Max(frame1, frame3), Math.Max(frame4, frame6));
            var target = frame1 | frame3 | frame4 | frame6;
            
            if ((fill & target) == target)
            {
                return true;
            }

            for (var i = 0; i < 6; i++)
            {
                var fillBefore = fill;
                
                // 5 6 7
                // 3   4
                // 0 1 2
                Fill8Bit(frame0, p1, p3, ref fill, frame);
                Fill8Bit(frame1, p0, p2, ref fill, frame);
                Fill8Bit(frame2, p1, p4, ref fill, frame);
                Fill8Bit(frame3, p0, p5, ref fill, frame);
                Fill8Bit(frame4, p2, p7, ref fill, frame);
                Fill8Bit(frame5, p3, p6, ref fill, frame);
                Fill8Bit(frame6, p5, p7, ref fill, frame);
                Fill8Bit(frame7, p4, p6, ref fill, frame);

                if (fillBefore == fill)
                {
                    break;
                }
            }

            return (fill & target) == target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Fill8Bit(byte origin, byte sibling0, byte sibling1, ref byte fill, byte frame)
        {
            Fill8Bit(origin, sibling0, ref fill, frame);
            Fill8Bit(origin, sibling1, ref fill, frame);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Fill8Bit(byte origin, byte sibling, ref byte fill, byte frame)
        {
            if ((origin & fill) != 0 && (sibling & frame) != 0)
            {
                fill |= sibling;
            }
        }
    }
}