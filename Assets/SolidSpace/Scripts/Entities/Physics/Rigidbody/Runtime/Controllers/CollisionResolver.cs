using System;
using System.Runtime.CompilerServices;
using SolidSpace.Mathematics;
using Unity.Mathematics;

namespace SolidSpace.Entities.Physics.Rigidbody
{
    public static class CollisionResolver
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ResolveIntersection(CenterRotationSize shapeA, CenterRotationSize shapeB, 
            out float2 motionA, out float2 motionB)
        {
            motionA = float2.zero;
            motionB = float2.zero;

            var childB = WorldToLocalSpace(shapeA, shapeB);
            var parentAChildB = new ParentChildCollision
            {
                parentHalfBounds = shapeA.size * 0.5f,
                childCenter = childB.center,
                childPoints = GetShapePointsClockwise(childB)
            };
            
            if (SeparateAxisExists(parentAChildB))
            {
                return false;
            }

            var childA = WorldToLocalSpace(shapeB, shapeA);
            var parentBChildA = new ParentChildCollision
            {
                parentHalfBounds = shapeB.size * 0.5f,
                childCenter = childA.center,
                childPoints = GetShapePointsClockwise(childA)
            };
            
            if (SeparateAxisExists(parentBChildA))
            {
                return false;
            }

            var childAMotion = ComputeChildMotion(parentBChildA);
            var childBMotion = ComputeChildMotion(parentAChildB);
            var weightA = Math.Abs(childAMotion.x) + Math.Abs(childAMotion.y);
            var weightB = Math.Abs(childBMotion.x) + Math.Abs(childBMotion.y);
            if (weightA > weightB)
            {
                FloatMath.SinCos(shapeB.rotation, out var sin, out var cos);
                motionA = FloatMath.Rotate(childAMotion, sin, cos) * 0.5f;
                motionB = -motionA;
            }
            else
            {
                FloatMath.SinCos(shapeA.rotation, out var sin, out var cos);
                motionB = FloatMath.Rotate(childBMotion, sin, cos) * 0.5f;
                motionA = -motionB;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static CenterRotationSize WorldToLocalSpace(CenterRotationSize parent, CenterRotationSize child)
        {
            child.rotation -= parent.rotation;
            child.center -= parent.center;
            FloatMath.SinCos(-parent.rotation, out var sin, out var cos);
            child.center = FloatMath.Rotate(child.center, sin, cos);
            
            return child;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SeparateAxisExists(ParentChildCollision collision)
        {
            var parentBounds = collision.parentHalfBounds;
            var childCenter = collision.childCenter;
            var p0 = collision.childPoints.p0;
            var p1 = collision.childPoints.p1;
            var p2 = collision.childPoints.p2;
            var p3 = collision.childPoints.p3;

            if (AllGreaterOrEqual(+parentBounds.x, childCenter.x, p0.x, p1.x, p2.x, p3.x))
            {
                return true;
            }

            if (AllLessOrEqual(-parentBounds.x, childCenter.x, p0.x, p1.x, p2.x, p3.x))
            {
                return true;
            }

            if (AllGreaterOrEqual(+parentBounds.y, childCenter.y, p0.y, p1.y, p2.y, p3.y))
            {
                return true;
            }

            if (AllLessOrEqual(-parentBounds.y, childCenter.y, p0.y, p1.y, p2.y, p3.y))
            {
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float2 ComputeChildMotion(ParentChildCollision collision)
        {
            var p0 = collision.childPoints.p0;
            var p1 = collision.childPoints.p1;
            var p2 = collision.childPoints.p2;
            var p3 = collision.childPoints.p3;
            var d0 = SegmentCast(collision.parentHalfBounds, collision.childCenter, ref p0, ref p2);
            var d1 = SegmentCast(collision.parentHalfBounds, collision.childCenter, ref p1, ref p3);

            if (d0  > d1)
            {
                return (p2 - p0) * d0;
            }
            
            return (p3 - p1) * d1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float SegmentCast(float2 parentHalfBounds, float2 childCenter, ref float2 p0, ref float2 p1)
        {
            if (FloatMath.Dot(p1 - p0, childCenter) < 0)
            {
                (p0, p1) = (p1, p0);
            }

            var delta = p1 - p0;
            
            var xMul = RaycastAxis(delta.x, p0.x,  parentHalfBounds.x * Math.Sign(childCenter.x));
            if (Math.Abs(p0.y + delta.y * xMul) > parentHalfBounds.y)
            {
                xMul = 0;
            }

            var yMul = RaycastAxis(delta.y, p0.y, parentHalfBounds.y * Math.Sign(childCenter.y));
            if (Math.Abs(p0.x + delta.x * yMul) > parentHalfBounds.x)
            {
                yMul = 0;
            }

            return Math.Max(xMul, yMul);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float RaycastAxis(float delta, float start, float axis)
        {
            if (Math.Abs(delta) < float.Epsilon)
            {
                return 0;
            }

            return (axis - start) / delta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point4 GetShapePointsClockwise(CenterRotationSize shape)
        {
            FloatMath.SinCos(shape.rotation, out var sin, out var cos);
            var halfSize = shape.size * 0.5f;
            return new Point4
            {
                p0 = shape.center + FloatMath.Rotate(-halfSize.x, -halfSize.y, sin, cos),
                p1 = shape.center + FloatMath.Rotate(-halfSize.x, +halfSize.y, sin, cos),
                p2 = shape.center + FloatMath.Rotate(+halfSize.x, +halfSize.y, sin, cos),
                p3 = shape.center + FloatMath.Rotate(+halfSize.x, -halfSize.y, sin, cos)
            };
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AllGreaterOrEqual(float t, float v0, float v1, float v2, float v3, float v4)
        {
            return (v0 >= t) && (v1 >= t) && (v2 >= t) && (v3 >= t) && (v4 >= t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AllLessOrEqual(float t, float v0, float v1, float v2, float v3, float v4)
        {
            return (v0 <= t) && (v1 <= t) && (v2 <= t) && (v3 <= t) && (v4 <= t);
        }
    }
}