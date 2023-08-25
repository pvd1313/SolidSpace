using System;
using System.Runtime.CompilerServices;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Colliders
{
    internal static class ColliderUtil
    {
        private const int MaxCellCount = ushort.MaxValue;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WorldToGrid(float x, float y, ColliderGrid grid, out int xInt, out int yInt)
        {
            xInt = ((int) x >> grid.power) - grid.anchor.x;
            yInt = ((int) y >> grid.power) - grid.anchor.y;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ColliderGrid ComputeGrid(NativeArray<FloatBounds> colliders, int colliderCount, ProfilingHandle profiler)
        {
            profiler.BeginSample("World Bounds");
            var worldBounds = ComputeWorldBounds(colliders, colliderCount, profiler);
            profiler.EndSample("World Bounds");
                
            profiler.BeginSample("Max Collider Size");
            var maxColliderSize = FindMaxColliderSize(colliders, colliderCount, profiler);
            profiler.EndSample("Max Collider Size");
                
            var cellSize = Math.Max(1, Math.Max(maxColliderSize.x, maxColliderSize.y));
            var cellPower = (int) Math.Ceiling(Math.Log(cellSize, 2));
            var worldMin = new int2((int) worldBounds.xMin >> cellPower, (int) worldBounds.yMin >> cellPower);
            var worldMax = new int2((int) worldBounds.xMax >> cellPower, (int) worldBounds.yMax >> cellPower);
                
            var cellTotal = (worldMax.x - worldMin.x + 1) * (worldMax.y - worldMin.y + 1);
            if (cellTotal > MaxCellCount)
            {
                cellSize = (1 << cellPower) / (float) Math.Sqrt(MaxCellCount / (float) cellTotal);
                cellPower = (int) Math.Ceiling(Math.Log(cellSize, 2));
                worldMin = new int2((int) worldBounds.xMin >> cellPower, (int) worldBounds.yMin >> cellPower);
                worldMax = new int2((int) worldBounds.xMax >> cellPower, (int) worldBounds.yMax >> cellPower);
            }

            return new ColliderGrid
            {
                anchor = worldMin,
                size = worldMax - worldMin + new int2(1, 1),
                power = cellPower
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FloatBounds ComputeWorldBounds(NativeArray<FloatBounds> colliders, int colliderCount, ProfilingHandle profiler)
        {
            var colliderJobCount = (int) Math.Ceiling(colliderCount / 128f);
            var colliderJoinedBounds = NativeMemory.CreateTempArray<FloatBounds>(colliderJobCount);
            var worldBoundsJob = new JoinBoundsJob
            {
                inBounds = colliders,
                inBoundsPerJob = 128,
                inTotalBounds = colliderCount,
                outBounds = colliderJoinedBounds
            };
            var worldBoundsJobHandle = worldBoundsJob.Schedule(colliderJobCount, 1);

            profiler.BeginSample("Jobs");
            worldBoundsJobHandle.Complete();
            profiler.EndSample("Jobs");

            profiler.BeginSample("Main Thread");
            var worldBounds = JoinBounds(colliderJoinedBounds);
            profiler.EndSample("Main Thread");

            colliderJoinedBounds.Dispose();

            return worldBounds;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static FloatBounds JoinBounds(NativeArray<FloatBounds> colliders)
        {
            if (colliders.Length == 0)
            {
                return default;
            }

            var entityBounds = colliders[0];
            var xMin = entityBounds.xMin;
            var xMax = entityBounds.xMax;
            var yMin = entityBounds.yMin;
            var yMax = entityBounds.yMax;
            var colliderCount = colliders.Length;
            for (var i = 1; i < colliderCount; i++)
            {
                entityBounds = colliders[i];

                if (entityBounds.xMin < xMin)
                {
                    xMin = entityBounds.xMin;
                }

                if (entityBounds.yMin < yMin)
                {
                    yMin = entityBounds.yMin;
                }

                if (entityBounds.xMax > xMax)
                {
                    xMax = entityBounds.xMax;
                }

                if (entityBounds.yMax > yMax)
                {
                    yMax = entityBounds.yMax;
                }
            }

            return new FloatBounds
            {
                xMin = xMin,
                xMax = xMax,
                yMin = yMin,
                yMax = yMax
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float2 FindMaxColliderSize(NativeArray<FloatBounds> colliders, int colliderCount, ProfilingHandle profiler)
        {
            var colliderJobCount = (int) Math.Ceiling(colliderCount / 128f);
            var colliderMaxSizes = NativeMemory.CreateTempArray<float2>(colliderJobCount);
            var colliderSizesJob = new FindMaxColliderSizeJob
            {
                inBounds = colliders,
                inBoundsPerJob = 128,
                inTotalBounds = colliderCount,
                outSizes = colliderMaxSizes
            };
            var colliderSizesJobHandle = colliderSizesJob.Schedule(colliderJobCount, 1);

            profiler.BeginSample("Jobs");
            colliderSizesJobHandle.Complete();
            profiler.EndSample("Jobs");

            profiler.BeginSample("Main Thread");
            var maxColliderSize = FindBoundsMaxSize(colliderMaxSizes);
            profiler.EndSample("Main Thread");

            colliderMaxSizes.Dispose();
                
            return maxColliderSize;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float2 FindBoundsMaxSize(NativeArray<float2> sizes)
        {
            if (sizes.Length == 0)
            {
                return default;
            }

            var size = sizes[0];
            var xMax = size.x;
            var yMax = size.y;
            var colliderCount = sizes.Length;

            for (var i = 1; i < colliderCount; i++)
            {
                size = sizes[i];

                if (size.x > xMax)
                {
                    xMax = size.x;
                }

                if (size.y > yMax)
                {
                    yMax = size.y;
                }
            }

            return new float2(xMax, yMax);
        }
    }
}