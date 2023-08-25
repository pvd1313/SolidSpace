using System;
using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Splitting
{
    [BurstCompile]
    public struct ShapeSeedJob : IJob
    {
        [ReadOnly] public NativeSlice<byte> inFrameBits;
        [ReadOnly] public int2 inFrameSize;
        
        [NativeDisableContainerSafetyRestriction] public NativeSlice<byte> outSeedMask;
        [NativeDisableContainerSafetyRestriction] public NativeSlice<ByteBounds> outSeedBounds;
        
        [NativeDisableContainerSafetyRestriction, WriteOnly] public NativeSlice<byte2> outConnections;
        [NativeDisableContainerSafetyRestriction, WriteOnly] public NativeSlice<ShapeSeedJobResult> outResult;

        private EShapeSeedResult _resultCode;
        private int _seedCount;
        private int _connectionCount;
        
        public void Execute()
        {
            var bytesPerLine = (int) Math.Ceiling(inFrameSize.x / 8f);
            Mask256 previousFill = default;
            
            outResult[0] = new ShapeSeedJobResult
            {
                code = EShapeSeedResult.None
            };
            
            for (var lineIndex = 0; lineIndex < inFrameSize.y; lineIndex++)
            {
                var frame = ReadMask(inFrameBits, lineIndex * bytesPerLine, bytesPerLine);
                var frameOffset = lineIndex * inFrameSize.x;
                for (var i = 0; i < inFrameSize.x; i++)
                {
                    if (!frame.HasBit((byte) i))
                    {
                        outSeedMask[frameOffset + i] = 0;
                    }
                }
                
                var newFill = ProjectPreviousFillOnFrame(frame, previousFill, lineIndex, lineIndex - 1);
                if (_resultCode != EShapeSeedResult.None)
                {
                    outResult[0] = new ShapeSeedJobResult
                    {
                        code = _resultCode
                    };
                    
                    return;
                }

                ProcessSeeds(frame, newFill, lineIndex);
                if (_resultCode != EShapeSeedResult.None)
                {
                    outResult[0] = new ShapeSeedJobResult
                    {
                        code = _resultCode
                    };
                    
                    return;
                }

                previousFill = frame;
            }

            outResult[0] = new ShapeSeedJobResult
            {
                code = EShapeSeedResult.Success,
                seedCount = _seedCount,
                connectionCount = _connectionCount,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessSeeds(Mask256 frame, Mask256 newFill, int frameY)
        {
            var seeds = frame ^ newFill;
            if (!seeds.HasAnyBit())
            {
                return;
            }
            
            var offset = frameY * inFrameSize.x;
            for (var i = 0; i < inFrameSize.x; i++)
            {
                if (!seeds.HasBit((byte) i))
                {
                    continue;
                }

                if (_seedCount >= outSeedBounds.Length - 1)
                {
                    _resultCode = EShapeSeedResult.TooManySeeds;
                    return;
                }

                var shapeId = _seedCount++ + 1;
                ByteBounds shape;
                shape.min.x = (byte) i;
                shape.min.y = (byte) frameY;
                shape.max.y = (byte) frameY;
                outSeedMask[offset + i] = (byte) shapeId;
                i++;
                while (i < inFrameSize.x && seeds.HasBit((byte) i))
                {
                    outSeedMask[offset + i] = (byte) shapeId;
                    i++;
                }

                shape.max.x = (byte) (i - 1);
                outSeedBounds[shapeId] = shape;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Mask256 ProjectPreviousFillOnFrame(Mask256 frame, Mask256 previousFill, int frameY, int previousFillY)
        {
            Mask256 newFill = default;

            var frameAndPreviousFill = frame & previousFill;
            if (!frameAndPreviousFill.HasAnyBit())
            {
                return newFill;
            }

            var currentFillOffset = frameY * inFrameSize.x;
            var previousLineOffset = previousFillY * inFrameSize.x;
            var lastCreatedLineBounds = new byte2(-1, -1);
            var lastCreatedLineId = -1;

            for (var i = 0; i < inFrameSize.x; i++)
            {
                if (!frameAndPreviousFill.HasBit((byte) i))
                {
                    continue;
                }

                var shapeId = outSeedMask[previousLineOffset + i];
                
                if (i >= lastCreatedLineBounds.x && i <= lastCreatedLineBounds.y)
                {
                    if (shapeId != lastCreatedLineId)
                    {
                        if (_connectionCount >= outConnections.Length)
                        {
                            _resultCode = EShapeSeedResult.TooManyConnections;
                            return default;
                        }
                        
                        outConnections[_connectionCount++] = new byte2(shapeId, lastCreatedLineId);
                    }

                    i++;
                    while (i <= lastCreatedLineBounds.y && previousFill.HasBit((byte) i))
                    {
                        i++;
                    }
                    
                    continue;
                }
                
                var shapeInfo = outSeedBounds[shapeId];
                shapeInfo.max.y = (byte) Math.Max(shapeInfo.max.y, frameY);
                outSeedMask[currentFillOffset + i] = shapeId;
                newFill.SetBitTrue((byte) i);
                        
                var toStart = i - 1;
                for (; toStart >= 0 && frame.HasBit((byte) toStart); toStart--)
                {
                    outSeedMask[currentFillOffset + toStart] = shapeId;
                    newFill.SetBitTrue((byte) toStart);
                }
                shapeInfo.min.x = (byte) Math.Min(shapeInfo.min.x, ++toStart);

                var toEnd = i + 1;
                for (; toEnd < inFrameSize.x && frame.HasBit((byte) toEnd); toEnd++)
                {
                    outSeedMask[currentFillOffset + toEnd] = shapeId;
                    newFill.SetBitTrue((byte) toEnd);
                }
                shapeInfo.max.x = (byte) Math.Max(shapeInfo.max.x, --toEnd);

                outSeedBounds[shapeId] = shapeInfo;
                lastCreatedLineBounds = new byte2(toStart, toEnd);
                lastCreatedLineId = shapeId;

                i++;
                while (i <= toEnd && previousFill.HasBit((byte) i))
                {
                    i++;
                }
            }

            return newFill;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly Mask256 ReadMask(NativeSlice<byte> bits, int offset, int count)
        {
            Mask256 resultMask = default;
            
            for (var i = 0; i < count; i++)
            {
                AddBits(ref resultMask, bits[offset + i], i * 8);
            }

            return resultMask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddBits(ref Mask256 mask, byte bits, int offset)
        {
            if (offset < 128)
            {
                if (offset < 64)
                {
                    mask.v0 |= ((long) bits) << offset;
                    return;
                }

                mask.v1 |= ((long) bits) << (offset - 64);
                return;
            }

            if (offset < 192)
            {
                mask.v2 |= ((long) bits) << (offset - 128);
                return;
            }

            mask.v3 |= ((long) bits) << (offset - 192);
        }
    }
}