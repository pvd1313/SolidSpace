using System.Runtime.CompilerServices;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Raycast
{
    [BurstCompile]
    public struct RaycastJob<T> : IJobParallelFor where T : struct, IRaycastBehaviour
    {
        public T behaviour;

        [ReadOnly] public NativeArray<ArchetypeChunk> inArchetypeChunks;
        [ReadOnly] public BakedColliders inColliders;
        [ReadOnly] public NativeArray<int> inWriteOffsets;

        [NativeDisableParallelForRestriction] public NativeArray<ushort> hitStack;
        [ReadOnly] public int hitStackSize;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<int> outCounts;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inArchetypeChunks[chunkIndex];
            var rayCount = chunk.Count;
            var writeOffset = inWriteOffsets[chunkIndex];
            var hitStackOffset = chunkIndex * hitStackSize;
            var jobHitCount = 0;
            
            behaviour.ReadChunk(chunk);

            for (var rayIndex = 0; rayIndex < rayCount && jobHitCount < rayCount; rayIndex++)
            {
                var ray = behaviour.GetRay(rayIndex);

                FloatBounds rayBounds;
                FloatMath.MinMax(ray.pos0.x, ray.pos1.x, out rayBounds.xMin, out rayBounds.xMax);
                FloatMath.MinMax(ray.pos0.y, ray.pos1.y, out rayBounds.yMin, out rayBounds.yMax);
                ColliderUtil.WorldToGrid(rayBounds.xMin, rayBounds.yMin, inColliders.grid, out var x0, out var y0);
                ColliderUtil.WorldToGrid(rayBounds.xMax, rayBounds.yMax, inColliders.grid, out var x1, out var y1);

                if (x1 < 0 || x0 >= inColliders.grid.size.x)
                {
                    continue;
                }
                
                if (y1 < 0 || y0 >= inColliders.grid.size.y)
                {
                    continue;
                }
                
                if ((x0 == x1) && (y0 == y1))
                {
                    var cellData = inColliders.cells[y0 * inColliders.grid.size.x + x0];
                    if (cellData.count == 0)
                    {
                        continue;
                    }

                    for (var i = 0; (i < cellData.count) && (jobHitCount < rayCount); i++)
                    {
                        var colliderIndex = inColliders.indices[cellData.offset + i];
                        
                        if (!RaycastCollider(rayBounds, colliderIndex))
                        {
                            continue;
                        }

                        var hitInfo = new RayHit
                        {
                            rayIndex = rayIndex,
                            colliderIndex = colliderIndex,
                            ray = ray,
                            writeOffset = writeOffset + jobHitCount
                        };
                        
                        if (behaviour.TryRegisterHit(hitInfo))
                        {
                            jobHitCount++;
                        }
                    }

                    continue;
                }

                var cellTotal = inColliders.grid.size.x * inColliders.grid.size.y;
                var rayHitCount = 0;
                var memoryLimitReached = false;
                for (var yOffset = y0; (yOffset <= y1) && (!memoryLimitReached); yOffset++)
                {
                    for (var xOffset = x0; (xOffset <= x1) && (!memoryLimitReached); xOffset++)
                    {
                        var cellIndex = yOffset * inColliders.grid.size.x + xOffset;
                        if (cellIndex < 0 || cellIndex >= cellTotal)
                        {
                            continue;
                        }

                        var cellData = inColliders.cells[cellIndex];
                        if (cellData.count == 0)
                        {
                            continue;
                        }

                        for (var j = 0; j < cellData.count; j++)
                        {
                            var colliderIndex = inColliders.indices[cellData.offset + j];
                            
                            if (HitStackContainsCollider(hitStackOffset, rayHitCount, colliderIndex))
                            {
                                continue;
                            }

                            if (!RaycastCollider(rayBounds, colliderIndex))
                            {
                                continue;
                            }

                            var hitInfo = new RayHit
                            {
                                rayIndex = rayIndex,
                                ray = ray,
                                colliderIndex = colliderIndex,
                                writeOffset = writeOffset + jobHitCount
                            };

                            if (!behaviour.TryRegisterHit(hitInfo))
                            {
                                continue;
                            }
                            
                            rayHitCount++; jobHitCount++;
                            if (jobHitCount >= rayCount || rayHitCount >= hitStackSize)
                            {
                                memoryLimitReached = true;
                                break;
                            }
                        }
                    }
                }
            }
            
            outCounts[chunkIndex] = jobHitCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool HitStackContainsCollider(int stackOffset, int hitCount, ushort collider)
        {
            if (hitCount == 0)
            {
                return false;
            }
            
            for (var i = 0; i < hitCount; i++)
            {
                if (hitStack[stackOffset + i] == collider)
                {
                    return true;
                }
            }

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly bool RaycastCollider(FloatBounds ray, ushort colliderIndex)
        {
            var colliderBounds = inColliders.bounds[colliderIndex];
            if (!FloatMath.BoundsOverlap(ray, colliderBounds))
            {
                return false;
            }

            var center = FloatMath.GetBoundsCenter(colliderBounds);
            var p0 = new float2(ray.xMin, ray.yMin) - center;
            var p1 = new float2(ray.xMax, ray.yMax) - center;
            var colliderShape = inColliders.shapes[colliderIndex];
            FloatMath.SinCos(-colliderShape.rotation, out var sin, out var cos);
            p0 = FloatMath.Rotate(p0, sin, cos);
            p1 = FloatMath.Rotate(p1, sin, cos);
            FloatMath.MinMax(p0.x, p1.x, out var xMin, out var xMax);
            FloatMath.MinMax(p0.y, p1.y, out var yMin, out var yMax);
            var halfSize = new float2(colliderShape.size.x / 2f, colliderShape.size.y / 2f);

            if (!FloatMath.BoundsOverlap(xMin, xMax, -halfSize.x, +halfSize.x))
            {
                return false;
            }

            if (!FloatMath.BoundsOverlap(yMin, yMax, -halfSize.y, +halfSize.y))
            {
                return false;
            }

            return true;
        }
    }
}