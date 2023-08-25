using System;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Utilities
{
    public struct ArchetypeChunksWithOffsets : IDisposable
    {
        public NativeArray<ArchetypeChunk> chunks;
        public NativeArray<int> chunkOffsets;
        public int chunkCount;
        public int entityCount;

        public void Dispose()
        {
            chunks.Dispose();
            chunkOffsets.Dispose();
        }
    }
}