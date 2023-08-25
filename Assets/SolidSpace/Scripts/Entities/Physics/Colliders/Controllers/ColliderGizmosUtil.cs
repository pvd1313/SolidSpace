using SolidSpace.Gizmos;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Colliders
{
    public static class ColliderGizmosUtil
    {
        public static void DrawColliders(GizmosHandle gizmos, BakedColliders colliders)
        {
            if (!gizmos.CheckEnabled())
            {
                return;
            }
            
            for (var i = 0; i < colliders.bounds.Length; i++)
            {
                var bounds = colliders.bounds[i];
                var shape = colliders.shapes[i];
                var center = new float2(bounds.xMin + bounds.xMax, bounds.yMin + bounds.yMax) / 2f;
                var angle = shape.rotation;
                
                gizmos.DrawWireRect(center, shape.size, angle);
            }
        }
        
        public static void DrawGrid(GizmosHandle gizmos, ColliderGrid worldGrid)
        {
            if (!gizmos.CheckEnabled())
            {
                return;
            }
            
            var cellSize = 1 << worldGrid.power;
            var cellCountX = worldGrid.size.x;
            var cellCountY = worldGrid.size.y;
            var worldMin = worldGrid.anchor * cellSize;
            var worldMax = (worldGrid.anchor + worldGrid.size) * cellSize;

            gizmos.DrawLine(worldMin.x, worldMin.y, worldMin.x, worldMax.y);
            gizmos.DrawLine(worldMin.x, worldMax.y, worldMax.x, worldMax.y);
            gizmos.DrawLine(worldMax.x, worldMax.y, worldMax.x, worldMin.y);
            gizmos.DrawLine(worldMax.x, worldMin.y, worldMin.x, worldMin.y);

            for (var i = 1; i < cellCountX; i++)
            {
                var p0 = new float2(worldMin.x + cellSize * i, worldMax.y);
                var p1 = new float2(worldMin.x + cellSize * i, worldMin.y);
                gizmos.DrawLine(p0, p1);
            }
            
            for (var i = 1; i < cellCountY; i++)
            {
                var p2 = new float2(worldMin.x, worldMin.y + i * cellSize);
                var p3 = new float2(worldMax.x, worldMin.y + i * cellSize);
                gizmos.DrawLine(p2, p3);
            }
        }
    }
}