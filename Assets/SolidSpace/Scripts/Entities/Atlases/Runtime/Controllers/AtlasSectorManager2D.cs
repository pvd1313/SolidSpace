using System;
using System.Collections.Generic;
using SolidSpace.Mathematics;
using Unity.Mathematics;

namespace SolidSpace.Entities.Atlases
{
    public class AtlasSectorManager2D
    {
        private struct FoundSectorInfo
        {
            public byte2 offset;
            public int stackIndex;
        }
        
        private readonly List<byte2>[] _emptySectors;
        private readonly byte _atlasPower;
        private readonly FoundSectorInfo[] _foundSectors;
        
        public AtlasSectorManager2D(int atlasSize)
        {
            _atlasPower = (byte) Math.Ceiling(Math.Log(atlasSize, 2));
            _emptySectors = new List<byte2>[_atlasPower + 1];
            _foundSectors = new FoundSectorInfo[3];
            
            for (var i = 0; i <= _atlasPower; i++)
            {
                _emptySectors[i] = new List<byte2>();
            }
            
            _emptySectors[_atlasPower].Add(byte2.zero);
        }

        public byte2 Allocate(int power)
        {
            var sectorStack = _emptySectors[power];
            if (sectorStack.Count > 0)
            {
                var lastIndex = sectorStack.Count - 1;
                var sector = sectorStack[lastIndex];
                sectorStack.RemoveAt(lastIndex);

                return sector;
            }

            for (var i = power + 1; i <= _atlasPower; i++)
            {
                sectorStack = _emptySectors[i];
                if (sectorStack.Count == 0)
                {
                    continue;
                }

                var lastIndex = sectorStack.Count - 1;
                var sector = sectorStack[lastIndex];
                sectorStack.RemoveAt(lastIndex);
                
                for (var j = i - 1; j >= power; j--)
                {
                    var size = 1 << (j - 2);
                    sectorStack = _emptySectors[j];
                    sectorStack.Add(new byte2
                    {
                        x = (byte)(sector.x + size),
                        y = sector.y
                    });
                    sectorStack.Add(new byte2
                    {
                        x = sector.x,
                        y = (byte)(sector.y + size)
                    });
                    sectorStack.Add(new byte2
                    {
                        x = (byte)(sector.x + size),
                        y = (byte)(sector.y + size)
                    });
                }

                return sector;
            }

            throw new OutOfMemoryException($"Failed to allocate sector {1 << power}x{1 << power}");
        }

        public void Release(byte2 offset, int power)
        {
            var stack = _emptySectors[power];
            var stackCount = stack.Count;
            if (stackCount < 3)
            {
                stack.Add(offset);
                return;
            }
            
            var size = 1 << (power - 2);
            var parentSize = size << 1;
            var atlasSize = 1 << (_atlasPower - 2);

            int2 reqDelta;
            var leftIsOdd = (atlasSize - offset.x) % parentSize == 0;
            var rightIsOdd = (atlasSize - offset.x + parentSize) % parentSize == 0;
            reqDelta.x = (leftIsOdd & rightIsOdd) ? size : -size;
            
            leftIsOdd = (atlasSize - offset.y) % parentSize == 0;
            rightIsOdd = (atlasSize - offset.y + parentSize) % parentSize == 0;
            reqDelta.y = (leftIsOdd & rightIsOdd) ? size : -size;

            var sectorsFound = 0;
            for (var i = 0; i < stackCount; i++)
            {
                var sibOffset = stack[i];
                var sibDelta = new int2(sibOffset.x - offset.x, sibOffset.y - offset.y);
                var sibDeltaEq = new bool2(sibDelta.x == reqDelta.x, sibDelta.y == reqDelta.y);
                var offsetEq = new bool2(sibOffset.x == offset.x, sibOffset.y == offset.y);
                if ((sibDeltaEq.x && offsetEq.y) || (sibDeltaEq.y && offsetEq.x) ||
                    (sibDeltaEq.x && sibDeltaEq.y))
                {
                    _foundSectors[sectorsFound++] = new FoundSectorInfo
                    {
                        offset = sibOffset,
                        stackIndex = i
                    };

                    if (sectorsFound < 3)
                    {
                        continue;
                    }

                    var xMin = offset.x;
                    var yMin = offset.y;
                    for (var j = 2; j >= 0; j--)
                    {
                        var sector = _foundSectors[j];
                        xMin = Math.Min(xMin, sector.offset.x);
                        yMin = Math.Min(yMin, sector.offset.y);
                        stack.RemoveAt(sector.stackIndex);
                    }
                    
                    Release(new byte2(xMin, yMin), power + 1);
                    
                    return;
                }
            }
            
            stack.Add(offset);
        }
    }
}