using SolidSpace.Entities.Atlases;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Entities.Health
{
    public interface IHealthAtlasSystem
    {
        public NativeArray<byte> Data { get; }
        
        public NativeSlice<AtlasChunk1D> Chunks { get; }
        
        public AtlasIndex16 Allocate(int2 size);
        
        public void Release(AtlasIndex16 index);
    }
}