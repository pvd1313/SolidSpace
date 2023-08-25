using System;
using SolidSpace.JobUtilities;
using Unity.Collections;

namespace SolidSpace.Gizmos
{
    internal struct ShapeStorage<T> : IDisposable where T : unmanaged
    {
        private NativeArray<T> _shapes;
        private int _shapeCount;
        private int _allocationSize;

        public ShapeStorage(int allocationSize)
        {
            _allocationSize = allocationSize;
            _shapes = NativeMemory.CreatePermArray<T>(allocationSize);
            _shapeCount = 0;
        }

        public void Add(T shape)
        {
            NativeMemory.MaintainPersistentArrayLength(ref _shapes, new ArrayMaintenanceData
            {
                copyOnResize = true,
                itemPerAllocation = _allocationSize,
                requiredCapacity = _shapeCount + 1
            });

            _shapes[_shapeCount++] = shape;
        }

        public NativeArray<T> GetShapes(out int shapeCount)
        {
            shapeCount = _shapeCount;
            
            return _shapes;
        }

        public void Clear()
        {
            _shapeCount = 0;
        }

        public void Dispose()
        {
            _shapes.Dispose();
        }
    }
}