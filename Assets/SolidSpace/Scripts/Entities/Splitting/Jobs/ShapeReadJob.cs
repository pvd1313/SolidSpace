using System;
using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace SolidSpace.Entities.Splitting
{
    [BurstCompile]
    public struct ShapeReadJob : IJob
    {
        [ReadOnly] public NativeSlice<ShapeSeedJobResult> inSeedJobResult;

        [NativeDisableContainerSafetyRestriction] public NativeSlice<ByteBounds> inOutBounds;
        [NativeDisableContainerSafetyRestriction] public NativeSlice<byte2> inOutConnections;
        
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeSlice<byte> outShapeRootSeeds;
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeSlice<int> outShapeCount;

        private Mask256 _connectionUsageMask;
        private Mask256 _seedUsageMask;
        private Mask256 _currentMask;
        
        public void Execute()
        {
            ShapeSeedJobResult seedJobResult = inSeedJobResult[0];
            if (seedJobResult.code != EShapeSeedResult.Success)
            {
                return;
            }
            
            var shapeCount = 0;

            for (var seed = 1; seed <= seedJobResult.seedCount; seed++)
            {
                if (_seedUsageMask.HasBit((byte) (seed - 1)))
                {
                    continue;
                }

                outShapeRootSeeds[shapeCount] = (byte) seed;
                var shapeBounds = inOutBounds[seed];
                _currentMask = default;
                _currentMask.SetBitTrue((byte)(seed - 1));
                var connectionFound = true;

                while (connectionFound)
                {
                    connectionFound = false;
                    
                    for (var connectionIndex = 0; connectionIndex < seedJobResult.connectionCount; connectionIndex++)
                    {
                        if (_connectionUsageMask.HasBit((byte) connectionIndex))
                        {
                            continue;
                        }

                        var connection = inOutConnections[connectionIndex];
                        if (_currentMask.HasBit((byte) (connection.x - 1)))
                        {
                            connectionFound = true;
                            _connectionUsageMask.SetBitTrue((byte) connectionIndex);
                            _seedUsageMask.SetBitTrue((byte) (connection.y - 1));
                            shapeBounds = JoinBounds(shapeBounds, inOutBounds[connection.y]);
                            inOutConnections[connectionIndex] = new byte2(seed, connection.y);
                            _currentMask.SetBitTrue((byte) (connection.y - 1));
                        }
                        else if (_currentMask.HasBit((byte) (connection.y - 1)))
                        {
                            connectionFound = true;
                            _connectionUsageMask.SetBitTrue((byte) connectionIndex);
                            _seedUsageMask.SetBitTrue((byte) (connection.x - 1));
                            shapeBounds = JoinBounds(shapeBounds, inOutBounds[connection.x]);
                            inOutConnections[connectionIndex] = new byte2(seed, connection.x);
                            _currentMask.SetBitTrue((byte) (connection.x - 1));
                        }
                    }
                }

                inOutBounds[shapeCount++] = shapeBounds;
            }

            outShapeCount[0] = shapeCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ByteBounds JoinBounds(ByteBounds a, ByteBounds b)
        {
            return new ByteBounds
            {
                min = new byte2
                {
                    x = Math.Min(a.min.x, b.min.x),
                    y = Math.Min(a.min.y, b.min.y),
                },
                max = new byte2
                {
                    x = Math.Max(a.max.x, b.max.x),
                    y = Math.Max(a.max.y, b.max.y)
                }
            };
        }
    }
}