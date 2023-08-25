using System;
using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Health
{
    public static class HealthUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasBit(NativeSlice<byte> health, int2 healthSize, int2 point)
        {
            var bytesPerLine = (int) Math.Ceiling(healthSize.x / 8f);
            var chunkIndex = bytesPerLine * point.y + point.x / 8;
            var chunkValue = health[chunkIndex];
            var pointMask = 1 << (point.x % 8);

            return (chunkValue & pointMask) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearBit(NativeSlice<byte> health, int2 spriteSize, int2 spritePoint)
        {
            var bytesPerLine = (int) Math.Ceiling(spriteSize.x / 8f);
            var chunkIndex = bytesPerLine * spritePoint.y + spritePoint.x / 8;
            var chunkValue = health[chunkIndex];
            var pointBitMask = 1 << (spritePoint.x % 8);

            health[chunkIndex] = (byte) (chunkValue & ~pointBitMask);
        }
        
        public static void TextureToHealth(NativeArray<Color32> texture, int2 size, NativeSlice<byte> output)
        {
            var bytesPerLine = (int) Math.Ceiling(size.x / 8f);
            var requiredByteCount = bytesPerLine * size.y;
            if (output.Length < requiredByteCount)
            {
                var message = $"{nameof(output)} must be at least {requiredByteCount}b to store frame {size.x}x{size.y}, but got {output.Length}b";
                throw new InvalidOperationException(message);
            }

            for (var y = 0; y < size.y; y++)
            {
                var textureOffset = size.x * y;
                var bitsOffset = bytesPerLine * y;
                
                for (var x = 0; x < size.x; x += 8)
                {
                    var bitChunk = 0;

                    for (var j = 0; j < 8 && (x + j < size.x); j++)
                    {
                        var color = texture[textureOffset + x + j];
                        if (color.r + color.g + color.b > 0)
                        {
                            bitChunk |= 1 << j;
                        }
                    }

                    output[bitsOffset + x / 8] = (byte) bitChunk;
                }
            }
        }

        public static void TextureToHealth(NativeArray<ColorRGB24> texture, int2 size, NativeSlice<byte> output)
        {
            var bytesPerLine = (int) Math.Ceiling(size.x / 8f);
            var requiredByteCount = bytesPerLine * size.y;
            if (output.Length < requiredByteCount)
            {
                var message = $"{nameof(output)} must be at least {requiredByteCount}b to store frame {size.x}x{size.y}, but got {output.Length}b";
                throw new InvalidOperationException(message);
            }

            for (var y = 0; y < size.y; y++)
            {
                var textureOffset = size.x * y;
                var bitsOffset = bytesPerLine * y;
                
                for (var x = 0; x < size.x; x += 8)
                {
                    var bitChunk = 0;

                    for (var j = 0; j < 8 && (x + j < size.x); j++)
                    {
                        var color = texture[textureOffset + x + j];
                        if (color.r + color.g + color.b > 0)
                        {
                            bitChunk |= 1 << j;
                        }
                    }

                    output[bitsOffset + x / 8] = (byte) bitChunk;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRequiredByteCount(int2 size)
        {
            return GetRequiredByteCount(size.x, size.y);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRequiredByteCount(int width, int height)
        {
            return (int) Math.Ceiling(width / 8f) * height;
        }
    }
}