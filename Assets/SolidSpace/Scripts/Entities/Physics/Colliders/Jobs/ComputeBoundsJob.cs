using System.Runtime.CompilerServices;
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
    internal struct ComputeBoundsJob : IJobParallelFor
    {
        private struct ColliderInfo
        {
            public byte archetypeIndex;
            public Entity entity;
            public FloatBounds bounds;
            public ColliderShape shape;
        }
        
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public NativeArray<int> inWriteOffsets;
        [ReadOnly] public NativeArray<byte> inArchetypeIndices;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<RectSizeComponent> sizeHandle;
        [ReadOnly] public ComponentTypeHandle<RotationComponent> rotationHandle;
        [ReadOnly] public EntityTypeHandle entityHandle;
        
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<FloatBounds> outBounds;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ColliderShape> outShapes;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<byte> outArchetypeIndices;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Entity> outEntities;

        public void Execute(int chunkIndex)
        {
            var chunk = inChunks[chunkIndex];
            var writeOffset = inWriteOffsets[chunkIndex];
            var archetypeIndex = inArchetypeIndices[chunkIndex];
            var positions = chunk.GetNativeArray(positionHandle);
            var sizes = chunk.GetNativeArray(sizeHandle);
            var entities = chunk.GetNativeArray(entityHandle);
            var entityCount = chunk.Count;
            
            if (chunk.Has(rotationHandle))
            {
                var rotations = chunk.GetNativeArray(rotationHandle);
                
                for (var i = 0; i < entityCount; i++)
                {
                    var center = positions[i].value;
                    var size = sizes[i].value;
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
                        archetypeIndex = archetypeIndex,
                        entity = entities[i],
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
                var center = positions[i].value;
                var size = sizes[i].value;
                var halfSize = new float2(size.x / 2f, size.y / 2f);

                WriteResult(writeOffset++, new ColliderInfo
                {
                    archetypeIndex = archetypeIndex,
                    entity = entities[i],
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteResult(int offset, ColliderInfo info)
        {
            outArchetypeIndices[offset] = info.archetypeIndex;
            outShapes[offset] = info.shape;
            outBounds[offset] = info.bounds;
            outEntities[offset] = info.entity;
        }
    }
}