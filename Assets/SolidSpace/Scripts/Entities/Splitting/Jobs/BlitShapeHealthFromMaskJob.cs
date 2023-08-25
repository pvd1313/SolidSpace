using System;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Splitting
{
    [BurstCompile]
    public struct BlitShapeHealthFromMaskJob : IJob
    {
        [ReadOnly] public NativeSlice<byte> inSourceSeedMask;
        [ReadOnly] public int2 inSourceSize;
        [ReadOnly] public int2 inSourceOffset;
        [ReadOnly] public int inTargetOffset;

        [ReadOnly] public byte inBlitShapeSeed;
        [ReadOnly] public int2 inBlitSize;

        [ReadOnly] public NativeSlice<byte2> inConnections;

        private Mask256 _shapeMask;

        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeSlice<byte> outTargetHealth;
        
        public void Execute()
        {
            _shapeMask = SplittingUtil.BuildShapeMask(inBlitShapeSeed, inConnections);

            var sourceOffset = inSourceOffset.y * inSourceSize.x + inSourceOffset.x;
            var targetOffset = inTargetOffset;
            var bytesPerLine = (int) Math.Ceiling(inBlitSize.x / 8f);

            for (var y = 0; y < inBlitSize.y; y++)
            {
                for (var x = 0; x < inBlitSize.x; x += 8)
                {
                    var bitChunk = 0;

                    for (var j = 0; j < 8 && (x + j < inBlitSize.x); j++)
                    {
                        var maskColor = inSourceSeedMask[sourceOffset + x + j];
                        if (maskColor != 0 && _shapeMask.HasBit((byte) (maskColor - 1)))
                        {
                            bitChunk |= 1 << j;
                        }
                    }

                    outTargetHealth[targetOffset + x / 8] = (byte) bitChunk;
                }

                sourceOffset += inSourceSize.x;
                targetOffset += bytesPerLine;
            }
        }
    }
}