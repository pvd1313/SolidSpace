using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Utilities
{
    public static class MeshRenderingUtil
    {
        private const int EntityPerMesh = 16384;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FillMesh(NativeArray<ArchetypeChunk> chunks, int startChunk, out int entityCount, out int chunkCount)
        {
            entityCount = 0;
            var chunkTotal = chunks.Length;
            var i = startChunk;
            for (; i < chunkTotal; i++)
            {
                var chunkEntityCount = chunks[i].Count;
                if (entityCount + chunkEntityCount > EntityPerMesh)
                {
                    break;
                }

                entityCount += chunkEntityCount;
            }

            chunkCount = i - startChunk;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawMesh(MeshDrawingData data)
        {
            Graphics.DrawMesh(data.mesh, data.matrix, data.material, data.layer, data.camera, data.subMeshIndex,
                data.properties, data.castShadows, data.receiveShadows, data.useLightProbes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Mesh CreateMesh(string name)
        {
            var mesh = new Mesh
            {
                name = name,
            };
            mesh.MarkDynamic();

            return mesh;
        }
    }
}