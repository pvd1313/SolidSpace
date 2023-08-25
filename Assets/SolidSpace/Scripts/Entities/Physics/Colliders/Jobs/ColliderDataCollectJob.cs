using SolidSpace.Entities.Components;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Colliders
{
    [BurstCompile]
    public struct ColliderDataCollectJob<T> : IJobParallelFor where T : struct, IColliderBakeBehaviour
    {
        private struct ColliderInfo
        {
            public FloatBounds bounds;
            public ColliderShape shape;
        }
        
        public T behaviour;

        [ReadOnly] public NativeArray<ArchetypeChunk> inArchetypeChunks;
        [ReadOnly] public NativeArray<int> inWriteOffsets;
        
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<RectSizeComponent> rectSizeHandle;
        [ReadOnly] public ComponentTypeHandle<RotationComponent> rotationHandle;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<FloatBounds> outBounds;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ColliderShape> outShapes;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inArchetypeChunks[chunkIndex];
            var writeOffset = inWriteOffsets[chunkIndex];
            var positions = chunk.GetNativeArray(positionHandle);
            var rectSizes = chunk.GetNativeArray(rectSizeHandle);
            var entityCount = chunk.Count;
            
            behaviour.OnProcessChunk(chunk);

            if (chunk.Has(rotationHandle))
            {
                var rotations = chunk.GetNativeArray(rotationHandle);
                
                for (var i = 0; i < entityCount; i++)
                {
                    behaviour.OnProcessChunkEntity(i, writeOffset);
                    
                    var center = positions[i].value;
                    var size = rectSizes[i].value;
                    var halfSize = new float2(size.x / 2f, size.y / 2f);
                    var angle = rotations[i].value;
                    
                    FloatMath.SinCos(angle, out var sin, out var cos);
                    var p0 = FloatMath.Rotate(-halfSize.x, -halfSize.y, sin, cos);
                    var p1 = FloatMath.Rotate(-halfSize.x, +halfSize.y, sin, cos);
                    var p2 = FloatMath.Rotate(+halfSize.x, +halfSize.y, sin, cos);
                    var p3 = FloatMath.Rotate(+halfSize.x, -halfSize.y, sin, cos);
                    FloatMath.MinMax(p0.x, p1.x, p2.x, p3.x, out var xMin, out var xMax);
                    FloatMath.MinMax(p0.y, p1.y, p2.y, p3.y, out var yMin, out var yMax);

                    WriteResult(writeOffset++, new ColliderInfo
                    {
                        bounds = new FloatBounds
                        {
                            xMin = center.x + xMin,
                            xMax = center.x + xMax,
                            yMin = center.y + yMin,
                            yMax = center.y + yMax
                        },
                        shape = new ColliderShape
                        {
                            size = size,
                            rotation = (half) angle
                        }
                    });
                }
                
                return;
            }
            
            for (var i = 0; i < entityCount; i++)
            {
                behaviour.OnProcessChunkEntity(i, writeOffset);
                
                var center = positions[i].value;
                var size = rectSizes[i].value;
                var halfSize = new float2(size.x / 2f, size.y / 2f);

                WriteResult(writeOffset++, new ColliderInfo
                {
                    bounds = new FloatBounds
                    {
                        xMin = center.x - halfSize.x,
                        xMax = center.x + halfSize.x,
                        yMin = center.y - halfSize.y,
                        yMax = center.y + halfSize.y
                    },
                    shape = new ColliderShape
                    {
                        size = size,
                        rotation = half.zero
                    }
                });
            }
        }

        private void WriteResult(int offset, ColliderInfo info)
        {
            outBounds[offset] = info.bounds;
            outShapes[offset] = info.shape;
        }
    }
}