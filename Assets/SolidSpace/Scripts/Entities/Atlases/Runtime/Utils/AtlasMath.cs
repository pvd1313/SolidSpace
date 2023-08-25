using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;

namespace SolidSpace.Entities.Atlases
{
    public static class AtlasMath
    {
        public const int Min1DEntitySize = 16;
        public const int Min2DEntitySize = 4;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeOffset(NativeSlice<AtlasChunk1D> chunks, AtlasIndex16 index)
        {
            return ComputeOffset(chunks[index.ReadChunkId()], index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeOffset(AtlasChunk1D chunk, AtlasIndex16 index)
        {
            return (chunk.offset << 2) + index.ReadItemId() * (1 << chunk.itemPower);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 ComputeOffset(NativeSlice<AtlasChunk2D> chunks, AtlasIndex16 index)
        {
            return ComputeOffset(chunks[index.ReadChunkId()], index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 ComputeOffset(AtlasChunk2D chunk, AtlasIndex16 index)
        {
            var itemId = index.ReadItemId();
            
            return new int2
            {
                x = (chunk.offset.x << 2) + (itemId &  3) * (1 << chunk.itemPower),
                y = (chunk.offset.y << 2) + (itemId >> 2) * (1 << chunk.itemPower)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 ComputeOffset(NativeSlice<AtlasChunk2D> chunks, AtlasIndex64 index)
        {
            return ComputeOffset(chunks[index.ReadChunkId()], index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 ComputeOffset(AtlasChunk2D chunk, AtlasIndex64 index)
        {
            var itemId = index.ReadItemId();

            return new int3
            {
                x = (chunk.offset.x << 2) + ((itemId & 0x10) >> 4) * (1 << chunk.itemPower),
                y = (chunk.offset.y << 2) + ((itemId & 0x20) >> 5) * (1 << chunk.itemPower),
                z = itemId & 0xF
            };
        }
    }
}