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

namespace SolidSpace.Entities.Rendering.Sprites
{
    internal class SpriteRenderingSystem : IInitializable, IUpdatable
    {
        private static readonly int MainTextureId = Shader.PropertyToID("_MainTex");
        private static readonly int FrameTextureId = Shader.PropertyToID("_FrameTex");

        private readonly IEntityManager _entityManager;
        private readonly SpriteMeshSystemConfig _config;
        private readonly ISpriteColorSystem _colorSystem;
        private readonly IProfilingManager _profilingManager;
        private readonly ISpriteFrameSystem _frameSystem;

        private EntityQuery _query;
        private VertexAttributeDescriptor[] _meshLayout;
        private List<Mesh> _meshes;
        private List<Mesh> _meshesForMeshArray;
        private Matrix4x4 _matrixDefault;
        private MeshUpdateFlags _meshUpdateFlags;
        private Material _material;
        private ProfilingHandle _profiler;

        public SpriteRenderingSystem(IEntityManager entityManager,
                                     SpriteMeshSystemConfig config,
                                     ISpriteColorSystem colorSystem,
                                     IProfilingManager profilingManager,
                                     ISpriteFrameSystem frameSystem)
        {
            _entityManager = entityManager;
            _config = config;
            _colorSystem = colorSystem;
            _profilingManager = profilingManager;
            _frameSystem = frameSystem;
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
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float16, 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 1)
            };
            _query = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(SpriteRenderComponent),
                typeof(RectSizeComponent),
                typeof(PrefabInstanceComponent)
            });
        }

        public void OnUpdate()
        {
            _profiler.BeginSample("Query chunks");
            var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
            _profiler.EndSample("Query chunks");
            
            _profiler.BeginSample("Compute offsets");
            var chunkTotal = chunks.Length;
            var chunkPerMesh = NativeMemory.CreateTempArray<int>(chunkTotal);
            var spritePerMesh = NativeMemory.CreateTempArray<int>(chunkTotal);
            var totalSpriteCount = 0;
            var meshCount = 0;
            var chunkIndex = 0;
            while (chunkIndex < chunkTotal)
            {
                MeshRenderingUtil.FillMesh(chunks, chunkIndex, out var spriteCount, out var chunkCount);
                chunkPerMesh[meshCount] = chunkCount;
                spritePerMesh[meshCount] = spriteCount;
                totalSpriteCount += spriteCount;
                chunkIndex += chunkCount;
                meshCount++;
            }
            _profiler.EndSample("Compute offsets");

            _profiler.BeginSample("Compute meshes");
            var meshDataArray = Mesh.AllocateWritableMeshData(meshCount);
            var computeJobHandles = NativeMemory.CreateTempArray<JobHandle>(meshCount);
            var chunkOffset = 0;
            for (var i = 0; i < meshCount; i++)
            {
                var meshData = meshDataArray[i];
                var spriteCount = spritePerMesh[i];
                meshData.SetVertexBufferParams(spriteCount * 4, _meshLayout);
                meshData.SetIndexBufferParams(spriteCount * 6, IndexFormat.UInt16);
                meshData.subMeshCount = 1;
                var subMeshDescriptor = new SubMeshDescriptor(0, spriteCount * 6);
                meshData.SetSubMesh(0, subMeshDescriptor, _meshUpdateFlags);

                var meshChunkCount = chunkPerMesh[i];
                var job = new SpriteMeshComputeJob
                {
                    inChunks = chunks,
                    positionHandle = _entityManager.GetComponentTypeHandle<PositionComponent>(true),
                    spriteHandle = _entityManager.GetComponentTypeHandle<SpriteRenderComponent>(true),
                    rotationHandle = _entityManager.GetComponentTypeHandle<RotationComponent>(true),
                    sizeHandle = _entityManager.GetComponentTypeHandle<RectSizeComponent>(true),
                    prefabHandle = _entityManager.GetComponentTypeHandle<PrefabInstanceComponent>(true),
                    inChunkCount = meshChunkCount,
                    inFirstChunkIndex = chunkOffset,
                    inColorAtlasChunks = _colorSystem.Chunks,
                    inColorAtlasSize = _colorSystem.AtlasSize,
                    inFrameAtlasChunks = _frameSystem.Chunks,
                    inFrameAtlasSize = _frameSystem.AtlasSize,
                    outIndices = meshData.GetIndexData<ushort>(),
                    outVertices = meshData.GetVertexData<SpriteVertexData>()
                };
                computeJobHandles[i] = job.Schedule();

                chunkOffset += meshChunkCount;
            }

            var computeHandle = JobHandle.CombineDependencies(computeJobHandles);
            computeHandle.Complete();
            _profiler.EndSample("Compute meshes");

            _profiler.BeginSample("Create meshes");
            for (var i = _meshes.Count; i < meshCount; i++)
            {
                var name = nameof(SpriteRenderingSystem) + "_" + i;
                _meshes.Add(MeshRenderingUtil.CreateMesh(name));
            }
            _meshesForMeshArray.Clear();
            for (var i = 0; i < meshCount; i++)
            {
                _meshesForMeshArray.Add(_meshes[i]);
            }
            _profiler.EndSample("Create meshes");
            
            _profiler.BeginSample("Apply and dispose writable mesh data");
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, _meshesForMeshArray, _meshUpdateFlags);
            _profiler.EndSample("Apply and dispose writable mesh data");
            
            _profiler.BeginSample("Draw mesh");
            
            _colorSystem.InsertAtlasIntoMaterial(_material, MainTextureId);
            _frameSystem.InsertAtlasIntoMaterial(_material, FrameTextureId);
            
            for (var i = 0; i < meshCount; i++)
            {
                MeshRenderingUtil.DrawMesh(new MeshDrawingData
                {
                    mesh = _meshes[i],
                    material = _material,
                    matrix = _matrixDefault
                });
            }
            _profiler.EndSample("Draw mesh");
            
            _profiler.BeginSample("Dispose native arrays");
            chunks.Dispose();
            chunkPerMesh.Dispose();
            spritePerMesh.Dispose();
            computeJobHandles.Dispose();
            _profiler.EndSample("Dispose native arrays");
            
            SpaceDebug.LogState("SpriteCount", totalSpriteCount);
            SpaceDebug.LogState("SpriteMeshCount", meshCount);
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