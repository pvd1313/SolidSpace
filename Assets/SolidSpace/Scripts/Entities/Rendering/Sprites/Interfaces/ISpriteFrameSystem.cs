using SolidSpace.Entities.Atlases;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public interface ISpriteFrameSystem
    {
        public NativeSlice<AtlasChunk2D> Chunks { get; }
        int2 AtlasSize { get;  }

        public AtlasIndex64 Allocate(int2 size);
        
        public void Release(AtlasIndex64 index);

        NativeArray<float> GetAtlasData(bool readOnly);
        
        void SetFrame(int3 index, bool frameExists);
        
        public void InsertAtlasIntoMaterial(Material material, int propertyId);
    }
}