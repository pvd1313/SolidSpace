using System.Collections.Generic;
using SolidSpace.Debugging;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Rendering.Utilities;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Profiling;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace SolidSpace.Entities.Rendering.Pixels
{
    internal class PixelRenderingSystem : IInitializable, IUpdatable
    {
        private readonly IEntityManager _entityManager;
        private readonly PixelMeshSystemConfig _config;
        private readonly IProfilingManager _profilingManager;

        private EntityQuery _query;
        private SquareVertices _square;
        private VertexAttributeDescriptor[] _meshLayout;
        private List<Mesh> _meshes;
        private List<Mesh> _meshesForMeshArray;
        private Matrix4x4 _matrixDefault;
        private MeshUpdateFlags _meshUpdateFlags;
        private Material _material;
        private ProfilingHandle _profiler;

        public PixelRenderingSystem(IEntityManager entityManager, PixelMeshSystemConfig config, IProfilingManager profilingManager)
        {
            _entityManager = entityManager;
            _config = config;
            _profilingManager = profilingManager;
        }
        
        public void OnInitialize()
        {
            _profiler = _profilingManager.GetHandle(this);
            _material = new Material(_config.Shader);
            _matrixDefault = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            _meshes = new List<Mesh>();
            _meshesForMeshArray = new List<Mesh>();
            _meshUpdateFlags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;
            _meshLayout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 2),
            };
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(PixelRenderComponent)
            });
            _square = new SquareVertices
            {
                point0 = new float2(-0.5f, -0.5f),
                point1 = new float2(-0.5f, +0.5f),
                point2 = new float2(+0.5f, +0.5f),
                point3 = new float2(+0.5f, -0.5f)
            };
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Query Chunks");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query Chunks");
            
            _profiler.BeginSample("Compute Offsets");
            var chunkTotal = chunks.Length;
            var chunkPerMesh = NativeMemory.CreateTempArray<int>(chunkTotal);
            var particlePerMesh = NativeMemory.CreateTempArray<int>(chunkTotal);
            var totalParticleCount = 0;
            var meshCount = 0;
            var chunkIndex = 0;
            while (chunkIndex < chunkTotal)
            {
                MeshRenderingUtil.FillMesh(chunks, chunkIndex, out var particleCount, out var chunkCount);
                chunkPerMesh[meshCount] = chunkCount;
                particlePerMesh[meshCount] = particleCount;
                totalParticleCount += particleCount;
                chunkIndex += chunkCount;
                meshCount++;
            }
            _profiler.EndSample("Compute Offsets");

            _profiler.BeginSample("Compute Meshes");
            var meshDataArray = Mesh.AllocateWritableMeshData(meshCount);
            var computeJobHandles = NativeMemory.CreateTempArray<JobHandle>(meshCount);
            var positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true);
            var chunkOffset = 0;
            for (var i = 0; i < meshCount; i++)
            {
                var meshData = meshDataArray[i];
                var particleCount = particlePerMesh[i];
                meshData.SetVertexBufferParams(particleCount * 4, _meshLayout);
                meshData.SetIndexBufferParams(particleCount * 6, IndexFormat.UInt16);
                meshData.subMeshCount = 1;
                var subMeshDescriptor = new SubMeshDescriptor(0, particleCount * 6);
                meshData.SetSubMesh(0, subMeshDescriptor, _meshUpdateFlags);

                var meshChunkCount = chunkPerMesh[i];
                var job = new PixelMeshComputeJob
                {
                    inChunks = chunks,
                    positionHandle = positionHandle,
                    inBakedSquare = _square,
                    inChunkCount = meshChunkCount,
                    inFirstChunkIndex = chunkOffset,
                    outIndices = meshData.GetIndexData<ushort>(),
                    outVertices = meshData.GetVertexData<PixelVertexData>()
                };
                computeJobHandles[i] = job.Schedule();

                chunkOffset += meshChunkCount;
            }

            var computeHandle = JobHandle.CombineDependencies(computeJobHandles);
            computeHandle.Complete();
            _profiler.EndSample("Compute Meshes");

            _profiler.BeginSample("Create meshes");
            for (var i = _meshes.Count; i < meshCount; i++)
            {
                var name = nameof(PixelRenderingSystem) + "_" + i;
                _meshes.Add( MeshRenderingUtil.CreateMesh(name));
            }
            _meshesForMeshArray.Clear();
            for (var i = 0; i < meshCount; i++)
            {
                _meshesForMeshArray.Add(_meshes[i]);
            }
            _profiler.EndSample("Create meshes");
            
            _profiler.BeginSample("Apply & Dispose Writable Mesh Data");
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _meshesForMeshArray, _meshUpdateFlags);
            _profiler.EndSample("Apply & Dispose Writable Mesh Data");
            
            _profiler.BeginSample("Draw Mesh");
            for (var i = 0; i < meshCount; i++)
            {
                MeshRenderingUtil.DrawMesh(new MeshDrawingData
                {
                    mesh = _meshes[i],
                    material = _material,
                    matrix = _matrixDefault
                });
            }
            _profiler.EndSample("Draw Mesh");
            
            _profiler.BeginSample("Dispose Arrays");
            chunks.Dispose();
            chunkPerMesh.Dispose();
            particlePerMesh.Dispose();
            computeJobHandles.Dispose();
            _profiler.EndSample("Dispose Arrays");
            
            SpaceDebug.LogState("ParticleCount", totalParticleCount);
            SpaceDebug.LogState("ParticleMeshCount", meshCount);
        }

        public void OnFinalize()
        {
            for (var i = 0; i < _meshes.Count; i++)
            {
                Object.Destroy(_meshes[i]);
                _meshes[i] = null;
            }
            Object.Destroy(_material);
        }
    }
}