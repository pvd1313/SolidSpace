using System.Runtime.CompilerServices;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Components;
using SolidSpace.Mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace SolidSpace.Entities.Rendering.Sprites
{
    [BurstCompile]
    internal struct SpriteMeshComputeJob : IJob
    {
        private struct Square
        {
            public float2 center;
            public float2 size;
            public float rotation;
            public half2 colorUVMin;
            public half2 colorUVMax;
            public half2 frameUVMin;
            public half2 frameUVMax;
            public float frameZValue;
        }
        
        [ReadOnly, NativeDisableContainerSafetyRestriction] public NativeArray<ArchetypeChunk> inChunks;
        [ReadOnly] public int inFirstChunkIndex;
        [ReadOnly] public int inChunkCount;
        [ReadOnly] public ComponentTypeHandle<PositionComponent> positionHandle;
        [ReadOnly] public ComponentTypeHandle<SpriteRenderComponent> spriteHandle;
        [ReadOnly] public ComponentTypeHandle<RotationComponent> rotationHandle;
        [ReadOnly] public ComponentTypeHandle<RectSizeComponent> sizeHandle;
        [ReadOnly] public ComponentTypeHandle<PrefabInstanceComponent> prefabHandle;
        [ReadOnly] public NativeSlice<AtlasChunk2D> inColorAtlasChunks;
        [ReadOnly] public int2 inColorAtlasSize;
        [ReadOnly] public NativeSlice<AtlasChunk2D> inFrameAtlasChunks;
        [ReadOnly] public int2 inFrameAtlasSize;

        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<SpriteVertexData> outVertices;
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<ushort> outIndices;
        
        public void Execute()
        {
            var lastChunkIndex = inFirstChunkIndex + inChunkCount;
            var vertexOffset = 0;
            var indexOffset = 0;
            var colorPixelSize = new float2(1f / inColorAtlasSize.x, 1f / inColorAtlasSize.y);
            var framePixelSize = new float2(1f / inFrameAtlasSize.x, 1f / inFrameAtlasSize.y);
            
            for (var chunkIndex = inFirstChunkIndex; chunkIndex < lastChunkIndex; chunkIndex++)
            {
                var archetypeChunk = inChunks[chunkIndex];
                var positions = archetypeChunk.GetNativeArray(positionHandle);
                var sizes = archetypeChunk.GetNativeArray(sizeHandle);
                var renders = archetypeChunk.GetNativeArray(spriteHandle);
                var prefabs = archetypeChunk.GetNativeArray(prefabHandle);
                var spriteCount = archetypeChunk.Count;
                
                var hasRotation = false;
                NativeArray<RotationComponent> rotations = default;
                
                if (archetypeChunk.Has(rotationHandle))
                {
                    rotations = archetypeChunk.GetNativeArray(rotationHandle);
                    hasRotation = true;
                }

                for (var i = 0; i < spriteCount; i++)
                {
                    var render = renders[i];
                    var size = sizes[i].value;
                    
                    var colorIndex = render.colorIndex;
                    var colorOffset = (float2) AtlasMath.ComputeOffset(inColorAtlasChunks, colorIndex);
                    colorOffset += prefabs[i].instanceOffset;

                    var frameIndex = render.frameIndex;
                    var frameOffset = AtlasMath.ComputeOffset(inFrameAtlasChunks, frameIndex);
                    var frameOffsetXY = new float2(frameOffset.x, frameOffset.y);
                    
                    FlushSquare(ref indexOffset, ref vertexOffset, new Square
                    {
                        center = positions[i].value,
                        size = size,
                        rotation = hasRotation ? rotations[i].value : 0f,
                        colorUVMin = (half2) (colorOffset * colorPixelSize),
                        colorUVMax = (half2) ((colorOffset + size) * colorPixelSize),
                        frameUVMin = (half2) (frameOffsetXY * framePixelSize),
                        frameUVMax = (half2) ((frameOffsetXY + size) * framePixelSize),
                        frameZValue = 1 << frameOffset.z
                    });
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushSquare(ref int indexOffset, ref int vertexOffset, Square square)
        {
            SpriteVertexData vertex;
            var halfSize = square.size * 0.5f;
            var colorUVMin = square.colorUVMin;
            var colorUVMax = square.colorUVMax;
            var frameUVMin = square.frameUVMin;
            var frameUVMax = square.frameUVMax;
            FloatMath.SinCos(square.rotation, out var sin, out var cos);

            vertex.frameZValue = square.frameZValue;
            
            vertex.position = square.center + FloatMath.Rotate(-halfSize.x, -halfSize.y, sin, cos);
            vertex.colorUV = colorUVMin;
            vertex.frameUV = frameUVMin;
            outVertices[vertexOffset + 0] = vertex;

            vertex.position = square.center + FloatMath.Rotate(-halfSize.x, +halfSize.y, sin, cos);
            vertex.colorUV.y = colorUVMax.y;
            vertex.frameUV.y = frameUVMax.y;
            outVertices[vertexOffset + 1] = vertex;
                
            vertex.position = square.center + FloatMath.Rotate(+halfSize.x, +halfSize.y, sin, cos);
            vertex.colorUV.x = colorUVMax.x;
            vertex.frameUV.x = frameUVMax.x;
            outVertices[vertexOffset + 2] = vertex;

            vertex.position = square.center + FloatMath.Rotate(+halfSize.x, -halfSize.y, sin, cos);
            vertex.colorUV.y = colorUVMin.y;
            vertex.frameUV.y = frameUVMin.y;
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