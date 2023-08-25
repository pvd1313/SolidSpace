using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SolidSpace.Entities.Atlases;
using SolidSpace.Entities.Components;
using SolidSpace.Entities.Despawn;
using SolidSpace.Entities.Health;
using SolidSpace.Entities.Rendering.Sprites;
using SolidSpace.Entities.World;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Prefabs
{
    internal class PrefabSystem : IInitializable, IUpdatable, IPrefabSystem, 
        IEntityDestructionHandler
    {
        public IReadOnlyList<ComponentType> PrefabComponents { get; private set; }

        private readonly PrefabSystemConfig _config;
        private readonly ISpriteColorSystem _colorSystem;
        private readonly ISpriteFrameSystem _frameSystem;
        private readonly IHealthAtlasSystem _healthSystem;
        private readonly IEntityManager _entityManager;

        private AtlasIndexManager1D16 _bakedHealthManager;
        private NativeArray<byte> _bakedHealth;
        private NativeArray<BakedPrefabData> _prefabs;
        private int _prefabCount;
        private EntityArchetype _prefabArchetype;
        private List<PrefabReplicationData> _replications;
        private List<ushort> _freePrefabIndices;
        private EntityQuery _destroyalQuery;

        public PrefabSystem(PrefabSystemConfig config, 
                                        ISpriteColorSystem colorSystem,
                                        ISpriteFrameSystem frameSystem,
                                        IHealthAtlasSystem healthSystem,
                                        IEntityManager entityManager)
        {
            _config = config;
            _colorSystem = colorSystem;
            _frameSystem = frameSystem;
            _healthSystem = healthSystem;
            _entityManager = entityManager;
        }

        public void OnInitialize()
        {
            _bakedHealthManager = new AtlasIndexManager1D16(_config.BakedHealthConfig);
            _bakedHealth = NativeMemory.CreatePermArray<byte>(_config.BakedHealthConfig.AtlasSize);
            _prefabs = NativeMemory.CreateTempArray<BakedPrefabData>(0);
            var components = new ComponentType[]
            {
                typeof(PositionComponent),
                typeof(RotationComponent),
                typeof(RectSizeComponent),
                typeof(RectColliderComponent),
                typeof(SpriteRenderComponent),
                typeof(HealthComponent),
                typeof(VelocityComponent),
                typeof(ActorComponent),
                typeof(RigidbodyComponent),
                typeof(PrefabInstanceComponent)
            };
            _prefabArchetype = _entityManager.CreateArchetype(components);
            _replications = new List<PrefabReplicationData>();
            _freePrefabIndices = new List<ushort>();
            _destroyalQuery = _entityManager.CreateEntityQuery(new ComponentType[]
            {
                typeof(PrefabInstanceComponent),
                typeof(DestroyedComponent)
            });

            PrefabComponents = components;
        }

        public void OnFinalize()
        {
            _bakedHealthManager.Dispose();
            _bakedHealth.Dispose();
            _prefabs.Dispose();
        }
        
        public ushort Create(Texture2D texture)
        {
            var size = new int2(texture.width, texture.height);
            var colorIndex = _colorSystem.Allocate(size);
            _colorSystem.Copy(texture, colorIndex);
            
            var pixels = texture.GetPixelData<ColorRGB24>(0);
            var healthByteCount = HealthUtil.GetRequiredByteCount(size);
            var healthIndex = _bakedHealthManager.Allocate(healthByteCount);
            var healthOffset = AtlasMath.ComputeOffset(_bakedHealthManager.Chunks, healthIndex);
            HealthUtil.TextureToHealth(pixels, size, _bakedHealth.Slice(healthOffset, healthByteCount));

            var prefabIndex = AllocatePrefabIndex();
            _prefabs[prefabIndex] = new BakedPrefabData
            {
                colorIndex = colorIndex,
                bakedHealthIndex = healthIndex,
                size = size,
                entityReferenceCount = 0,
                hasManagedReference = true,
                isCreated = true
            };

            return prefabIndex;
        }

        private ushort AllocatePrefabIndex()
        {
            var freeIndexCount = _freePrefabIndices.Count;
            if (freeIndexCount > 0)
            {
                var index = _freePrefabIndices[freeIndexCount - 1];
                _freePrefabIndices.RemoveAt(freeIndexCount - 1);
                return index;
            }
            
            NativeMemory.EnsureCapacity(ref _prefabs, _prefabCount + 1);
            
            return (ushort) _prefabCount++;
        }

        public void ScheduleRelease(ushort prefabIndex)
        {
            var prefabData = GetAndValidate(prefabIndex);
            if (!prefabData.hasManagedReference)
            {
                throw new InvalidOperationException($"Prefab with index {prefabIndex} is already released.");
            }

            prefabData.hasManagedReference = false;
            UpdateOrReleasePrefab(prefabData, prefabIndex);
        }

        public void OnBeforeEntitiesDestroyed()
        {
            var prefabs = _destroyalQuery.ToComponentDataArray<PrefabInstanceComponent>(Allocator.TempJob);
            foreach (var prefabComponent in prefabs)
            {
                var prefabData = _prefabs[prefabComponent.prefabIndex];
                prefabData.entityReferenceCount--;
                UpdateOrReleasePrefab(prefabData, prefabComponent.prefabIndex);
            }

            prefabs.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateOrReleasePrefab(BakedPrefabData data, ushort prefabIndex)
        {
            if (!data.hasManagedReference && data.entityReferenceCount == 0)
            {
                _bakedHealthManager.Release(data.bakedHealthIndex);
                _colorSystem.Release(data.colorIndex);
                _prefabs[prefabIndex] = default;
                _freePrefabIndices.Add(prefabIndex);
            }
            else
            {
                _prefabs[prefabIndex] = data;
            }
        }

        public void Instantiate(ushort prefabIndex, float2 position, float rotation)
        {
            var prefabData = GetAndValidate(prefabIndex);
            var frameIndex = _frameSystem.Allocate(prefabData.size);
            var healthIndex = _healthSystem.Allocate(prefabData.size);
            var entity = _entityManager.CreateEntity(_prefabArchetype);
            
            _entityManager.SetComponentData(entity, new PositionComponent
            {
                value = position
            });
            _entityManager.SetComponentData(entity, new RectSizeComponent
            {
                value = new half2((half)prefabData.size.x, (half)prefabData.size.y)
            });
            _entityManager.SetComponentData(entity, new RotationComponent
            {
                value = rotation
            });
            _entityManager.SetComponentData(entity, new SpriteRenderComponent
            {
                colorIndex = prefabData.colorIndex,
                frameIndex = frameIndex
            });
            _entityManager.SetComponentData(entity, new PrefabInstanceComponent
            {
                prefabIndex = prefabIndex,
                instanceOffset = byte2.zero
            });
            _entityManager.SetComponentData(entity, new HealthComponent
            {
                index = healthIndex
            });
            _entityManager.SetComponentData(entity, new ActorComponent
            {
                isActive = false
            });
            
            var bakedHealthOffset = AtlasMath.ComputeOffset(_bakedHealthManager.Chunks, prefabData.bakedHealthIndex);
            var bakedHealthSize = HealthUtil.GetRequiredByteCount(prefabData.size);
            var healthOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks, healthIndex);
            var healthAtlas = _healthSystem.Data;
            for (var i = 0; i < bakedHealthSize; i++)
            {
                healthAtlas[healthOffset + i] = _bakedHealth[bakedHealthOffset + i];
            }
            
            new BlitHealthToFrameJob
            {
                inHealth = _bakedHealth.Slice(bakedHealthOffset, bakedHealthSize),
                inHealthSize = prefabData.size,
                inFrameAtlasSize = _frameSystem.AtlasSize,
                inOutFrameAtlas = _frameSystem.GetAtlasData(false),
                inFrameAtlasOffset = AtlasMath.ComputeOffset(_frameSystem.Chunks, frameIndex),
            }.Run();

            prefabData.entityReferenceCount++;
            _prefabs[prefabIndex] = prefabData;
        }

        public void ScheduleReplication(PrefabReplicationData replicationData)
        {
            _replications.Add(replicationData);
        }

        public void OnUpdate()
        {
            foreach (var replication in _replications)
            {
                var parentEntity = replication.parent;
                var parentPrefab = _entityManager.GetComponentData<PrefabInstanceComponent>(parentEntity);
                var parentPosition = _entityManager.GetComponentData<PositionComponent>(parentEntity).value;
                var parentRotation = _entityManager.GetComponentData<RotationComponent>(parentEntity).value;
                var parentSize = _entityManager.GetComponentData<RectSizeComponent>(parentEntity).value;
                var prefabData = GetAndValidate(parentPrefab.prefabIndex);
                
                var childEntity = _entityManager.CreateEntity(_prefabArchetype);
                var childBounds = replication.childBounds;
                var childHealthIndex = replication.childHealth;
                var childSize = childBounds.GetSize();
                
                _entityManager.SetComponentData(childEntity, new RectSizeComponent
                {
                    value = new half2((half) childSize.x, (half) childSize.y)
                });
                
                var childLocalPosX = (childBounds.max.x + childBounds.min.x + 1) * 0.5f - parentSize.x * 0.5f;
                var childLocalPosY = (childBounds.max.y + childBounds.min.y + 1) * 0.5f - parentSize.y * 0.5f;
                FloatMath.SinCos(parentRotation, out var childSin, out var childCos);
                _entityManager.SetComponentData(childEntity, new PositionComponent
                {
                    value = FloatMath.Rotate(childLocalPosX, childLocalPosY, childSin, childCos) + parentPosition
                });
                
                _entityManager.SetComponentData(childEntity, new RotationComponent
                {
                    value = parentRotation
                });
                
                _entityManager.SetComponentData(childEntity, new HealthComponent
                {
                    index = childHealthIndex
                });

                var childFrameIndex = _frameSystem.Allocate(childSize);
                var healthOffset = AtlasMath.ComputeOffset(_healthSystem.Chunks, childHealthIndex);
                var healthSize = HealthUtil.GetRequiredByteCount(childSize);
                new BlitHealthToFrameJob
                {
                    inHealth = _healthSystem.Data.Slice(healthOffset, healthSize),
                    inOutFrameAtlas = _frameSystem.GetAtlasData(false),
                    inFrameAtlasOffset = AtlasMath.ComputeOffset(_frameSystem.Chunks, childFrameIndex),
                    inFrameAtlasSize = _frameSystem.AtlasSize,
                    inHealthSize = childSize,
                }.Run();
                _entityManager.SetComponentData(childEntity, new SpriteRenderComponent
                {
                    colorIndex = prefabData.colorIndex,
                    frameIndex = childFrameIndex
                });
                
                _entityManager.SetComponentData(childEntity, new PrefabInstanceComponent
                {
                    prefabIndex = parentPrefab.prefabIndex,
                    instanceOffset = parentPrefab.instanceOffset + childBounds.min
                });

                prefabData.entityReferenceCount++;
                _prefabs[parentPrefab.prefabIndex] = prefabData;
            }

            _replications.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BakedPrefabData GetAndValidate(ushort index)
        {
            if (index > _prefabCount)
            {
                throw new InvalidOperationException($"Prefab by index {index} is not created. (index out of range)");
            }

            var data = _prefabs[index];
            if (!data.isCreated)
            {
                throw new InvalidOperationException($"Prefab by index {index} is not created. (already released)");
            }

            return data;
        }
    }
}