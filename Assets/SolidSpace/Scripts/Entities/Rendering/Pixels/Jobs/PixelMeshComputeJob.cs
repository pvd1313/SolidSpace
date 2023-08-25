using SolidSpace.Entities.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace SolidSpace.Entities.Rendering.Pixels
{
    [BurstCompile]
    internal struct PixelMeshComputeJob : IJob
    {
        [ReadOnly, NativeDisableContainerSafetyRestriction] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public int inFirstChunkIndex;
        [ReadOnly] public int inChunkCount;
        [ReadOnly] public SquareVertices inBakedSquare;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;

        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<PixelVertexData> outVertices;
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<ushort> outIndices;
        
        public void Execute()
        {
            var lastChunkIndex = inFirstChunkIndex + inChunkCount;
            var vertexOffset = 0;
            var indexOffset = 0;
            for (var chunkIndex = inFirstChunkIndex; chunkIndex < lastChunkIndex; chunkIndex++)
            {
                var chunk = inChunks[chunkIndex];
                var positions = chunk.GetNativeArray(positionHandle);
                var particleCount = chunk.Count;
                for (var i = 0; i < particleCount; i++)
                {
                    var position = positions[i].value;
                
                    PixelVertexData vertex;

                    vertex.position = position + inBakedSquare.point0;
                    outVertices[vertexOffset + 0] = vertex;
                
                    vertex.position = position + inBakedSquare.point1;
                    outVertices[vertexOffset + 1] = vertex;
                
                    vertex.position = position + inBakedSquare.point2;
                    outVertices[vertexOffset + 2] = vertex;
                
                    vertex.position = position + inBakedSquare.point3;
                    outVertices[vertexOffset + 3] = vertex;

                    outIndices[indexOffset + 0] = (ushort) (vertexOffset + 0);
                    outIndices[indexOffset + 1] = (ushort) (vertexOffset + 1);
                    outIndices[indexOffset + 2] = (ushort) (vertexOffset + 2);
                    outIndices[indexOffset + 3] = (ushort) (vertexOffset + 0);
                    outIndices[indexOffset + 4] = (ushort) (vertexOffset + 2);
                    outIndices[indexOffset + 5] = (ushort) (vertexOffset + 3);

                    vertexOffset += 4;
                    indexOffset += 6;
                }
            }
        }
    }
}