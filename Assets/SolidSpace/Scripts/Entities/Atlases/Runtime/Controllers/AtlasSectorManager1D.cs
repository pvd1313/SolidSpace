using System;
using System.Collections.Generic;

namespace SolidSpace.Entities.Atlases
{
    public class AtlasSectorManager1D
    {
        private readonly List<ushort>[] _emptySectors;
        private readonly byte _atlasPower;
        
        public AtlasSectorManager1D(int atlasSize)
        {
            _atlasPower = (byte) Math.Ceiling(Math.Log(atlasSize, 2));
            _emptySectors = new List<ushort>[_atlasPower + 1];
            
            for (var i = 0; i <= _atlasPower; i++)
            {
                _emptySectors[i] = new List<ushort>();
            }
            
            _emptySectors[_atlasPower].Add(0);
        }

        public ushort Allocate(int power)
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
                    sectorStack.Add((ushort) (sector + size));
                }

                return sector;
            }

            throw new OutOfMemoryException($"Failed to allocate sector with size {1 << power}");
        }

        public void Release(ushort offset, int power)
        {
            var stack = _emptySectors[power];
            var stackCount = stack.Count;
            if (stackCount == 0)
            {
                stack.Add(offset);
                return;
            }

            var size = 1 << (power - 2);
            var parentSize = size << 1;
            var atlasSize = 1 << (_atlasPower - 2);
            var leftIsOdd = (atlasSize - offset) % parentSize == 0;
            var rightIsOdd = (atlasSize - offset + parentSize) % parentSize == 0;
            var requiredDelta = (leftIsOdd & rightIsOdd) ? size : -size;
            for (var i = 0; i < stackCount; i++)
            {
                var siblingOffset = stack[i];
                var siblingDelta = siblingOffset - offset;
                if (siblingDelta == requiredDelta)
                {
                    stack.RemoveAt(i);
                    Release(Math.Min(offset, siblingOffset), power + 1);
                    return;
                }
            }
            
            stack.Add(offset);
        }
    }
}