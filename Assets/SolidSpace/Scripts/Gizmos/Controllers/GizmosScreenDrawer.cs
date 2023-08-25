using SolidSpace.Gizmos.Shapes;
using Unity.Mathematics;
using UnityEngine;

namespace SolidSpace.Gizmos
{
    internal class GizmosScreenDrawer
    {
        private static Camera Camera;
        
        public static void BeginDraw(Camera camera)
        {
            Camera = camera;
            
            GL.PushMatrix();
            GL.LoadPixelMatrix();
            GL.Begin(GL.TRIANGLES);
        }
        
        public static void EndDraw()
        {
            GL.End();
            GL.PopMatrix();
        }

        public static void DrawSquares(ShapeStorage<Square> squareStorage)
        {
            var squares = squareStorage.GetShapes(out var squareCount);
            for (var i = 0; i < squareCount; i++)
            {
                var square = squares[i];
                GL.Color(square.color);
                var screenCenter = Camera.WorldToScreenPoint(new Vector3(square.center.x, square.center.y, 0));
                var center = new float2(screenCenter.x, screenCenter.y);
                var halfSize = square.size / 2f;
                var p0 = center + new float2(-halfSize, -halfSize);
                var p1 = center + new float2(-halfSize, +halfSize);
                var p2 = center + new float2(+halfSize, +halfSize);
                var p3 = center + new float2(+halfSize, -halfSize);
                GL_Triangle(p0, p1, p2);
                GL_Triangle(p2, p3, p0);
            }
        }

        private static void GL_Triangle(float2 p0, float2 p1, float2 p2)
        {
            GL.Vertex3(p0.x, p0.y, 0);
            GL.Vertex3(p1.x, p1.y, 0);
            GL.Vertex3(p2.x, p2.y, 0);
        }
    }
}