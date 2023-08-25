using System;
using System.Runtime.CompilerServices;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Physics.Colliders;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Rigidbody
{
    [BurstCompile]
    public struct RigidbodyCollisionJob : IJobParallelFor
    {
        [ReadOnly] public BakedColliders inColliders;
        [ReadOnly] public NativeArray<ArchetypeChunk> inArchetypeChunks;
        [ReadOnly] public float inMotionMultiplier;

        [ReadOnly] public ComponentTypeHandle<RigidbodyComponent> rigidbodyHandle;

        [NativeDisableParallelForRestriction] public NativeArray<ushort> hitStack;
        [ReadOnly] public int hitStackSize;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<float2> outMotion;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inArchetypeChunks[chunkIndex];
            var entityCount = chunk.Count;
            var rigidbodies = chunk.GetNativeArray(rigidbodyHandle);
            var hitStackOffset = chunkIndex * hitStackSize;

            for (var entityIndex = 0; entityIndex < entityCount; entityIndex++)
            {
                var thisIndex = rigidbodies[entityIndex].colliderIndex;
                var thisShape = inColliders.shapes[thisIndex];
                var thisBounds = inColliders.bounds[thisIndex];
                var thisCenter = FloatMath.GetBoundsCenter(thisBounds);
                
                ColliderUtil.WorldToGrid(thisBounds.xMin, thisBounds.yMin, inColliders.grid, out var x0, out var y0);
                ColliderUtil.WorldToGrid(thisBounds.xMax, thisBounds.yMax, inColliders.grid, out var x1, out var y1);

                var motion = float2.zero;
                
                if (x0 == x1 && y0 == y1)
                {
                    var cellData = inColliders.cells[y0 * inColliders.grid.size.x + x0];
                    if (cellData.count > 1)
                    {
                        for (var i = 0; i < cellData.count; i++)
                        {
                            var otherIndex = inColliders.indices[cellData.offset + i];
                            if (otherIndex == thisIndex)
                            {
                                continue;
                            }

                            var otherBounds = inColliders.bounds[otherIndex];
                            if (!FloatMath.BoundsOverlap(thisBounds, otherBounds))
                            {
                                continue;
                            }

                            var otherCenter = FloatMath.GetBoundsCenter(otherBounds);
                            var otherShape = inColliders.shapes[otherIndex];
                            var shapeA = new CenterRotationSize(thisCenter, thisShape.rotation, thisShape.size);
                            var shapeB = new CenterRotationSize(otherCenter, otherShape.rotation, otherShape.size);

                            var centerEpsilon = new float2(thisIndex > otherIndex ? 0.01f : -0.01f, 0);
                            shapeA.center += centerEpsilon;
                            shapeB.center -= centerEpsilon;
                            
                            if (!CollisionResolver.ResolveIntersection(shapeA, shapeB, out var motionA, out _))
                            {
                                continue;
                            }

                            var direction = thisCenter - otherCenter;
                            var directionMag = FloatMath.Magnitude(direction);
                            if (directionMag > float.Epsilon)
                            {
                                motion += direction / directionMag * FloatMath.Magnitude(motionA) * inMotionMultiplier;
                            }
                            else
                            {
                                motion += motionA * inMotionMultiplier;
                            }
                        }
                    }
                }
                else
                {
                    var hitCount = 0;

                    for (var yOffset = y0; (yOffset <= y1) && (hitCount < hitStackSize); yOffset++)
                    {
                        for (var xOffset = x0; (xOffset <= x1) && (hitCount < hitStackSize); xOffset++)
                        {
                            var cellData = inColliders.cells[yOffset * inColliders.grid.size.x + xOffset];
                            if (cellData.count < 2)
                            {
                                continue;
                            }

                            for (var i = 0; i < (cellData.count) && (hitCount < hitStackSize); i++)
                            {
                                var otherIndex = inColliders.indices[cellData.offset + i];
                                if (otherIndex == thisIndex)
                                {
                                    continue;
                                }

                                if (HitStackContainsCollider(hitStackOffset, hitCount, otherIndex))
                                {
                                    continue;
                                }

                                var otherBounds = inColliders.bounds[otherIndex];
                                if (!FloatMath.BoundsOverlap(thisBounds, otherBounds))
                                {
                                    continue;
                                }

                                var otherCenter = FloatMath.GetBoundsCenter(otherBounds);
                                var otherShape = inColliders.shapes[otherIndex];
                                var shapeA = new CenterRotationSize(thisCenter, thisShape.rotation, thisShape.size);
                                var shapeB = new CenterRotationSize(otherCenter, otherShape.rotation, otherShape.size);
                                
                                var centerEpsilon = new float2(thisIndex > otherIndex ? 0.01f : -0.01f, 0);
                                shapeA.center += centerEpsilon;
                                shapeB.center -= centerEpsilon;
                                
                                if (!CollisionResolver.ResolveIntersection(shapeA, shapeB, out var motionA, out _))
                                {
                                    continue;
                                }

                                hitStack[hitStackOffset + hitCount++] = otherIndex;

                                var direction = thisCenter - otherCenter;
                                var directionMag = FloatMath.Magnitude(direction);
                                if (directionMag > float.Epsilon)
                                {
                                    motion += direction / directionMag * FloatMath.Magnitude(motionA) * inMotionMultiplier;
                                }
                                else
                                {
                                    motion += motionA * inMotionMultiplier;
                                }
                            }
                        }
                    }
                }

                var motionMag = FloatMath.Magnitude(motion);
                var maxMotion = Math.Max(thisShape.size.x, thisShape.size.y) * 0.5f;
                if (motionMag > maxMotion)
                {
                    motion = motion / motionMag * maxMotion;
                }
                
                outMotion[thisIndex] = motion;
            }
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
    }
}