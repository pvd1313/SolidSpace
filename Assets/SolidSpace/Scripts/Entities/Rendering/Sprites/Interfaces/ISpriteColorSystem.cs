using SolidSpace.Entities.Atlases;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Rendering.Sprites
{
    public interface ISpriteColorSystem
    {
        public NativeSlice<AtlasChunk2D> Chunks { get; }
        public NativeSlice<ushort> ChunksOccupation { get; }
        public int2 AtlasSize { get; }

        public AtlasIndex16 Allocate(int2 size);
        
        public void Release(AtlasIndex16 index);
        
        public void Copy(Texture2D source, AtlasIndex16 target);

        public void InsertAtlasIntoMaterial(Material material, int propertyId);
    }
}