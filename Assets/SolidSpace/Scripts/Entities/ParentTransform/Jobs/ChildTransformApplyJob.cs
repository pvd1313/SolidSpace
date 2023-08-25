using SolidSpace.Entities.Components;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.ParentTransform
{
    [BurstCompile]
    public struct ChildTransformApplyJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ParentData> inParentData;
        [ReadOnly] public NativeArray<ArchetypeChunk> inChildChunks;

        [ReadOnly] public ComponentTypeHandle<LocalPositionComponent> localPositionHandle;
        [ReadOnly] public ComponentTypeHandle<ChildComponent> childHandle;
        public ComponentTypeHandle<PositionComponent> positionHandle;
        public ComponentTypeHandle<RotationComponent> rotationHandle;
        
        public void Execute(int chunkIndex)
        {
            var chunk = inChildChunks[chunkIndex];
            var entityCount = chunk.ChunkEntityCount;
            var childHandles = chunk.GetNativeArray(childHandle);
            var worldPositions = chunk.GetNativeArray(positionHandle);
            
            var localPositions = chunk.GetNativeArray(localPositionHandle);
            var worldRotations = chunk.GetNativeArray(rotationHandle);

            for (var i = 0; i < entityCount; i++)
            {
                var handle = childHandles[i].parentHandle;
                var parent = inParentData[handle.index];

                var localOffset = localPositions.IsCreated ? FloatMath.Rotate(localPositions[i].value, parent.rotation) : default;
                worldPositions[i] = new PositionComponent
                {
                    value = parent.position + localOffset
                };

                if (worldRotations.IsCreated)
                {
                    worldRotations[i] = new RotationComponent
                    {
                        value = parent.rotation
                    };
                }
            }
        }
    }
}