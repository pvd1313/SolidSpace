using System;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SolidSpace.Entities.Physics.Colliders
{
    [BurstCompile]
    internal struct ChunkCollidersJob : IJobParallelFor
    {
        [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<FloatBounds> inColliderBounds;
        [ReadOnly] public int inColliderTotalCount;
        [ReadOnly] public int inColliderPerJob;
        [ReadOnly] public ColliderGrid inWorldGrid;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ChunkedCollider> outColliders;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<int> outColliderCounts;
        
        public void Execute(int jobIndex)
        {
            var startIndex = jobIndex * inColliderPerJob;
            var endIndex = Math.Min(startIndex + inColliderPerJob, inColliderTotalCount);
            var colliderCount = 0;
            var writeOffset = jobIndex * inColliderPerJob * 4;
            var worldSize = inWorldGrid.size;

            for (var i = startIndex; i < endIndex; i++)
            {
                var bounds = inColliderBounds[i];
                ColliderUtil.WorldToGrid(bounds.xMin, bounds.yMin, inWorldGrid, out var x0, out var y0);
                ColliderUtil.WorldToGrid(bounds.xMax, bounds.yMax, inWorldGrid, out var x1, out var y1);
                var anchorChunk = y0 * worldSize.x + x0;

                ChunkedCollider chunkedCollider;
                chunkedCollider.colliderIndex = (ushort) i;
                
                chunkedCollider.chunkIndex = (ushort) anchorChunk;
                outColliders[writeOffset + colliderCount++] = chunkedCollider;

                if (x0 != x1)
                {
                    chunkedCollider.chunkIndex++;
                    outColliders[writeOffset + colliderCount++] = chunkedCollider;

                    if (y0 != y1)
                    {
                        chunkedCollider.chunkIndex = (ushort) (anchorChunk + worldSize.x);
                        outColliders[writeOffset + colliderCount++] = chunkedCollider;

                        chunkedCollider.chunkIndex++;
                        outColliders[writeOffset + colliderCount++] = chunkedCollider;
                        
                        continue;
                    }
                }

                if (y0 != y1)
                {
                    chunkedCollider.chunkIndex = (ushort) (anchorChunk + worldSize.x);
                    outColliders[writeOffset + colliderCount++] = chunkedCollider;
                }
            }

            outColliderCounts[jobIndex] = colliderCount;
        }
    }
}