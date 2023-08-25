using System;
using System.Collections.Generic;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Mathematics;

namespace SolidSpace.Entities.Atlases
{
    public class AtlasIndexManager2D64 : IDisposable
    {
        private const int BufferSize = 64;
        
        public NativeSlice<AtlasChunk2D> Chunks { get; private set; }
        public NativeSlice<ulong> ChunksOccupation { get; private set; }

        private readonly AtlasSectorManager2D _sectorManager;
        private readonly int _maxEntityPower;
        private readonly int _minEntityPower;
        private readonly List<ushort>[] _partiallyFilledChunks;
        private readonly Stack<ushort> _freeChunkIndices;
        
        private NativeArray<AtlasChunk2D> _chunks;
        private NativeArray<ulong> _chunksOccupation;

        public AtlasIndexManager2D64(Atlas2DConfig config)
        {
            _sectorManager = new AtlasSectorManager2D(config.AtlasSize);
            _minEntityPower = (int) CeilLog2(config.MinItemSize);
            _maxEntityPower = (int) CeilLog2(config.MaxItemSize);
            _freeChunkIndices = new Stack<ushort>();
            _partiallyFilledChunks = new List<ushort>[_maxEntityPower + 1];
            for (var i = _minEntityPower; i <= _maxEntityPower; i++)
            {
                _partiallyFilledChunks[i] = new List<ushort>();
            }

            _chunks = NativeMemory.CreatePermArray<AtlasChunk2D>(0);
            _chunksOccupation = NativeMemory.CreatePermArray<ulong>(0);
            
            Chunks = new NativeSlice<AtlasChunk2D>(_chunks, 0, 0);
            ChunksOccupation = new NativeSlice<ulong>(_chunksOccupation, 0, 0);
        }

        public AtlasIndex64 Allocate(int2 size)
        {
            var maxSize = Math.Max(size.x, size.y);
            var entityPower = (byte) Math.Max(_minEntityPower, CeilLog2(maxSize));
            if (entityPower > _maxEntityPower)
            {
                var message = $"Size {1 << entityPower} is more than allowed {1 << _maxEntityPower}";
                throw new InvalidOperationException(message);
            }

            var chunkStack = _partiallyFilledChunks[entityPower];
            if (chunkStack.Count == 0)
            {
                CreateChunk(entityPower);
            }
            
            var chunkId = chunkStack[0];
            var freeIndexMask = ~_chunksOccupation[chunkId];
            var itemId = BinaryMath.GetFirstBitIndex64(freeIndexMask);
            freeIndexMask &= ~(1ul << itemId);
            
            _chunksOccupation[chunkId] = ~freeIndexMask;
            
            if (freeIndexMask == 0)
            {
                chunkStack.RemoveAt(0);
            }

            return new AtlasIndex64(chunkId, itemId);
        }

        public void Release(AtlasIndex64 index)
        {
            index.Read(out var chunkId, out var itemId);
            
            var chunk = _chunks[chunkId];
            var chunkOccupation = _chunksOccupation[chunkId];
            var itemPower = chunk.itemPower;

            if ((chunkOccupation & (1ul << itemId)) == 0)
            {
                throw new InvalidOperationException($"Can not release {index}. It is not allocated.");
            }
            
            if (chunkOccupation == ulong.MaxValue)
            {
                _partiallyFilledChunks[itemPower].Insert(0, (ushort) chunkId);
            }
            
            chunkOccupation &= ~(1ul << itemId);
            _chunksOccupation[chunkId] = chunkOccupation;
            
            if (chunkOccupation == 0)
            {
                _partiallyFilledChunks[itemPower].Remove((ushort) chunkId);
                _sectorManager.Release(chunk.offset, itemPower + 1);
                _freeChunkIndices.Push((ushort) chunkId);
            }
        }

        private void CreateChunk(int itemPower)
        {
            if (_freeChunkIndices.Count == 0)
            {
                CreateChunkIndices(BufferSize);
            }
            
            var chunkId = _freeChunkIndices.Pop();
            var sector = _sectorManager.Allocate(itemPower + 1);
            var chunk = new AtlasChunk2D
            {
                offset = sector,
                itemPower = (byte) itemPower
            };

            _chunks[chunkId] = chunk;
            _chunksOccupation[chunkId] = 0;
            _partiallyFilledChunks[itemPower].Add(chunkId);
        }
        
        private void CreateChunkIndices(int newIndexCount)
        {
            var rule = new ArrayMaintenanceData
            {
                requiredCapacity = _chunks.Length + newIndexCount,
                itemPerAllocation = newIndexCount,
                copyOnResize = true
            };
            NativeMemory.MaintainPersistentArrayLength(ref _chunks, rule);
            NativeMemory.MaintainPersistentArrayLength(ref _chunksOccupation, rule);
            Chunks = new NativeSlice<AtlasChunk2D>(_chunks, 0, _chunks.Length);
            ChunksOccupation = new NativeSlice<ulong>(_chunksOccupation, 0, _chunksOccupation.Length);

            for (var i = _chunks.Length - newIndexCount; i < _chunks.Length; i++)
            {
                _chunks[i] = default;
                _chunksOccupation[i] = 0;
                _freeChunkIndices.Push((ushort) i);
            }
        }

        private static double CeilLog2(double a)
        {
            return Math.Ceiling(Math.Log(a, 2));
        }

        public void Dispose()
        {
            _chunks.Dispose();
            _chunksOccupation.Dispose();
        }
    }
}