using System.Runtime.CompilerServices;
using SolidSpace.Gizmos.Shapes;
using SolidSpace.Mathematics;
using Unity.Mathematics;
using UnityEngine;
using Rect = SolidSpace.Gizmos.Shapes.Rect;

namespace SolidSpace.Gizmos
{
    internal static class GizmosWireDrawer
    {
        public static void BeginDraw()
        {
            GL.PushMatrix();
            GL.MultMatrix(Matrix4x4.identity);
            GL.Begin(GL.LINES);
        }

        public static void EndDraw()
        {
            GL.End();
            GL.PopMatrix();
        }
        
        public static void DrawLines(ShapeStorage<Line> lineStorage)
        {
            var lines = lineStorage.GetShapes(out var lineCount);
            for (var i = 0; i < lineCount; i++)
            {
                var line = lines[i];
                GL.Color(line.color);
                GL_Line(line.start, line.end);
            }
        }

        public static void DrawRects(ShapeStorage<Rect> rectStorage)
        {
            var rects = rectStorage.GetShapes(out var rectCount);
            for (var i = 0; i < rectCount; i++)
            {
                var rect = rects[i];
                GL.Color(rect.color);
                var center = rect.center;
                var halfSize = new float2(rect.size.x / 2f, rect.size.y / 2f);
                FloatMath.SinCos(rect.rotationRad, out var sin, out var cos);
                var p0 = center + FloatMath.Rotate(-halfSize.x, -halfSize.y, sin, cos);
                var p1 = center + FloatMath.Rotate(-halfSize.x, +halfSize.y, sin, cos);
                var p2 = center + FloatMath.Rotate(+halfSize.x, +halfSize.y, sin, cos);
                var p3 = center + FloatMath.Rotate(+halfSize.x, -halfSize.y, sin, cos);
                GL_Quad(p0, p1, p2, p3);
            }
        }

        public static void DrawSquares(ShapeStorage<Square> rectStorage)
        {
            var squares = rectStorage.GetShapes(out var squareCount);
            for (var i = 0; i < squareCount; i++)
            {
                var square = squares[i];
                GL.Color(square.color);
                var center = square.center;
                var halfSize = square.size / 2f;
                var p0 = center + new float2(-halfSize, -halfSize);
                var p1 = center + new float2(-halfSize, +halfSize);
                var p2 = center + new float2(+halfSize, +halfSize);
                var p3 = center + new float2(+halfSize, -halfSize);
                GL_Quad(p0, p1, p2, p3);
            }
        }

        public static void DrawPolygons(ShapeStorage<Polygon> polygonStorage)
        {
            var polygons = polygonStorage.GetShapes(out var polygonCount);
            for (var i = 0; i < polygonCount; i++)
            {
                var polygon = polygons[i];
                GL.Color(polygon.color);
                var step = FloatMath.TwoPI / polygon.topology;
                var forward = new float2(polygon.radius, 0);
                var startPoint = polygon.center + forward;
                var prevPoint = startPoint;

                for (var j = 1; j < polygon.topology; j++)
                {
                    FloatMath.SinCos(step * j, out var sin, out var cos);
                    var newPoint = polygon.center + FloatMath.Rotate(forward, sin, cos);
                    GL_Line(prevPoint, newPoint);
                    prevPoint = newPoint;
                }
                
                GL_Line(prevPoint, startPoint);
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GL_Quad(float2 p0, float2 p1, float2 p2, float2 p3)
        {
            GL_Line(p0, p1);
            GL_Line(p1, p2);
            GL_Line(p2, p3);
            GL_Line(p3, p0);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void GL_Line(float2 start, float2 end)
        {
            GL.Vertex3(start.x, start.y, 0);
            GL.Vertex3(end.x, end.y, 0);
        }
    }
}