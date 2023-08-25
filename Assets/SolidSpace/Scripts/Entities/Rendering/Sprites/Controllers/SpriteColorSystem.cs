using System;
using SolidSpace.Entities.Atlases;
using SolidSpace.GameCycle;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    internal class SpriteColorSystem : ISpriteColorSystem, IInitializable
    {
        public int2 AtlasSize { get; private set; }
        public NativeSlice<AtlasChunk2D> Chunks => _indexManager.Chunks;
        public NativeSlice<ushort> ChunksOccupation => _indexManager.ChunksOccupation;
        
        private readonly SpriteAtlasConfig _config;
        
        private AtlasIndexManager2D16 _indexManager;

        private Texture2D _texture;

        public SpriteColorSystem(SpriteAtlasConfig config)
        {
            _config = config;
        }
        
        public void OnInitialize()
        {
            var atlasSize = _config.AtlasConfig.AtlasSize;

            _texture = new Texture2D(atlasSize, atlasSize, TextureFormat.RGB24, false, false);
            _texture.name = nameof(SpriteColorSystem);
            _texture.filterMode = FilterMode.Point;
            
            _indexManager = new AtlasIndexManager2D16(_config.AtlasConfig);

            AtlasSize = new int2(atlasSize, atlasSize);
        }
        
        public void OnFinalize()
        {
            _indexManager.Dispose();
            UnityEngine.Object.Destroy(_texture);
            _texture = null;
        }

        public AtlasIndex16 Allocate(int2 size)
        {
            return _indexManager.Allocate(size);
        }

        public void Release(AtlasIndex16 index)
        {
            _indexManager.Release(index);
        }

        public void InsertAtlasIntoMaterial(Material material, int propertyId)
        {
            material.SetTexture(propertyId, _texture);
        }

        public void Copy(Texture2D source, AtlasIndex16 target)
        {
            if (source.format != TextureFormat.RGB24)
            {
                var message = $"Expected texture with format RGB24 but got {source.format}.";
                throw new InvalidOperationException(message);
            }

            var chunk = _indexManager.Chunks[target.ReadChunkId()];
            var itemMaxSize = 1 << chunk.itemPower;
            if (source.width > itemMaxSize || source.height > itemMaxSize)
            {
                var message = $"Expected texture with size less that {itemMaxSize}x{itemMaxSize}, but got {source.width}x{source.height}.";
                throw new InvalidOperationException(message);
            }
            
            var offset = AtlasMath.ComputeOffset(chunk, target);
            Graphics.CopyTexture(source, 0, 0, 0, 0, source.width, source.height, 
                _texture, 0, 0, offset.x, offset.y);
        }
    }
}