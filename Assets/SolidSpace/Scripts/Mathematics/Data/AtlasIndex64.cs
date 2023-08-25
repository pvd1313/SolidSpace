using System.Runtime.CompilerServices;

namespace SolidSpace.Mathematics
{
    public struct AtlasIndex64
    {
        private ushort _value;

        public AtlasIndex64(int chunkId, int itemId)
        {
            _value = (ushort) (((chunkId & 0x3FF) << 6) + (itemId & 0x3F));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(out int chunkId, out int itemId)
        {
            chunkId = ReadChunkId();
            itemId = ReadItemId();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadChunkId()
        {
            return _value >> 6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadItemId()
        {
            return _value & 0x3F;
        }

        public override string ToString()
        {
            Read(out var chunkId, out var itemId);
            
            return $"({chunkId}, {itemId})";
        }
    }
}