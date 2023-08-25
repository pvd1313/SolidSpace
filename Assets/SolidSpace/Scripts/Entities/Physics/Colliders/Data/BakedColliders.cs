using System;
using SolidSpace.Mathematics;
using Unity.Collections;

namespace SolidSpace.Entities.Physics.Colliders
{
    public struct BakedColliders : IDisposable
    {
        public ColliderGrid grid;
        public NativeArray<ColliderShape> shapes;
        public NativeArray<FloatBounds> bounds;
        public NativeArray<ColliderListPointer> cells;
        public NativeArray<ushort> indices;

        public void Dispose()
        {
            shapes.Dispose();
            bounds.Dispose();
            cells.Dispose();
            indices.Dispose();
        }
    }
}