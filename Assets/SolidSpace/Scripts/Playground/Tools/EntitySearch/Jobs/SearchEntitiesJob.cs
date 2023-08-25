using SolidSpace.Entities.Components;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Playground.Tools.EntitySearch
{
    [BurstCompile]
    public struct SearchEntitiesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public float2 inSearchPoint;
        [ReadOnly] public float inSearchRadius;
        [ReadOnly] public NativeArray<int> inRadiusWriteOffsets;
        
        [ReadOnly] public EntityTypeHandle entityHandle;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;

        [WriteOnly] public NativeArray<float2> outNearestPositions;
        [WriteOnly] public NativeArray<Entity> outNearestEntities;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Entity> outEntitiesInRadius;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<float2> outPointsInRadius;
        [WriteOnly] public NativeArray<int> outInRadiusCounts;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inChunks[chunkIndex];
            var positions = chunk.GetNativeArray(positionHandle);
            var entities = chunk.GetNativeArray(entityHandle);
            var entityCount = chunk.Count;
            var minDistance = float.MaxValue;
            var minPosition = float2.zero;
            var inRadiusCount = 0;
            var inRadiusWriteOffset = inRadiusWriteOffsets[chunkIndex];
            Entity minEntity = default;

            for (var i = 0; i < entityCount; i++)
            {
                var position = positions[i].value;
                var entity = entities[i];
                var distance = FloatMath.Distance(inSearchPoint, position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minEntity = entity;
                    minPosition = position;
                }

                if (distance <= inSearchRadius)
                {
                    var writeOffset = inRadiusWriteOffset + inRadiusCount++;
                    outEntitiesInRadius[writeOffset] = entity;
                    outPointsInRadius[writeOffset] = position;
                }
            }

            outNearestPositions[chunkIndex] = minPosition;
            outNearestEntities[chunkIndex] = minEntity;
            outInRadiusCounts[chunkIndex] = inRadiusCount;
        }
    }
}