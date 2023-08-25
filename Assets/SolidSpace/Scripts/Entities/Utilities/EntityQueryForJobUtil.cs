using System.Runtime.CompilerServices;
using SolidSpace.JobUtilities;
using Unity.Collections;
using Unity.Entities;

namespace SolidSpace.Entities.Utilities
{
    public static class EntityQueryForJobUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArchetypeChunksWithOffsets QueryWithOffsets(EntityQuery entityQuery)
        {
            var chunks = entityQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            return ComputeOffsets(chunks);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ArchetypeChunksWithOffsets ComputeOffsets(NativeArray<ArchetypeChunk> chunks)
        {
            var offsets = NativeMemory.CreateTempArray<int>(chunks.Length);
            var chunkCount = chunks.Length;
            var entityCount = 0;
            
            for (var i = 0; i < chunkCount; i++)
            {
                offsets[i] = entityCount;
                entityCount += chunks[i].Count;
            }

            return new ArchetypeChunksWithOffsets
            {
                chunks = chunks,
                chunkOffsets = offsets,
                chunkCount = chunkCount,
                entityCount = entityCount
            };
        }

        public static ArchetypeChunksWithOffsets PartialQueryWithOffsets(EntityQuery entityQuery, 
            int cycleLength, ref int cycleIndex)
        {
            var chunks = entityQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var offsets = NativeMemory.CreateTempArray<int>(chunks.Length);
            var chunksLength = chunks.Length;
            
            var chunkCount = 0;
            var entityCount = 0;
            for (var i = 0; i < chunksLength; i++)
            {
                var chunk = chunks[i];
                if (chunk.GetHashCode() % cycleLength != cycleIndex)
                {
                    continue;
                }

                offsets[chunkCount] = entityCount;
                chunks[chunkCount] = chunk;
                entityCount += chunk.Count;
                chunkCount++;
            }

            cycleIndex = (cycleIndex + 1) % cycleLength;

            return new ArchetypeChunksWithOffsets
            {
                chunks = chunks,
                chunkOffsets = offsets,
                chunkCount = chunkCount,
                entityCount = entityCount
            };
        }
    }
}