using System;
using SolidSpace.Entities.Atlases;
using SolidSpace.GameCycle;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public class SpriteFrameSystem : ISpriteFrameSystem, IInitializable, IUpdatable
    {
        public int2 AtlasSize { get; private set; }
        public NativeSlice<AtlasChunk2D> Chunks => _indexManager.Chunks;

        private readonly SpriteAtlasConfig _config;
        
        private Texture2D _texture;
        private AtlasIndexManager2D64 _indexManager;
        private bool _isTextureDirty;

        public SpriteFrameSystem(SpriteAtlasConfig config)
        {
            _config = config;
        }

        public void OnInitialize()
        {
            var atlasSize = _config.AtlasConfig.AtlasSize;

            _texture = new Texture2D(atlasSize, atlasSize, TextureFormat.RFloat, false, true);
            _texture.name = nameof(SpriteFrameSystem);
            _texture.filterMode = FilterMode.Point;

            _indexManager = new AtlasIndexManager2D64(_config.AtlasConfig);

            AtlasSize = new int2(atlasSize, atlasSize);
            
            new FillNativeArrayJob<float>
            {
                inValue = 0,
                inTotalItem = atlasSize * atlasSize,
                inItemPerJob = 1024,
                outNativeArray = GetAtlasData(false)
            }.Schedule((int) Math.Ceiling(atlasSize * atlasSize / 1024f), 4).Complete();
        }

        public void OnFinalize()
        {
            _indexManager.Dispose();
            UnityEngine.Object.Destroy(_texture);
            _texture = null;
        }
        
        public void OnUpdate()
        {
            if (_isTextureDirty)
            {
                _isTextureDirty = false;
                _texture.Apply();
            }
        }

        public NativeArray<float> GetAtlasData(bool readOnly)
        {
            if (!readOnly)
            {
                _isTextureDirty = true;
            }

            return _texture.GetPixelData<float>(0);
        }

        public void SetFrame(int3 index, bool frameExists)
        {
            _isTextureDirty = true;
            var offset = index.y * AtlasSize.x + index.x;
            var texture = _texture.GetPixelData<float>(0);
            var value = (int) texture[offset];
            
            if (frameExists)
            {
                value |= 1 << index.z;
            }
            else
            {
                value &= ~(1 << index.z);
            }

            texture[offset] = value;
        }

        public void InsertAtlasIntoMaterial(Material material, int propertyId)
        {
            material.SetTexture(propertyId, _texture);
        }

        public AtlasIndex64 Allocate(int2 size)
        {
            return _indexManager.Allocate(size);
        }

        public void Release(AtlasIndex64 index)
        {
            _indexManager.Release(index);
        }
    }
}