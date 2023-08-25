using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace SolidSpace.Mathematics
{
    public static class FloatMath
    {
        public const float PI = (float) Math.PI;
        public const float TwoPI = (float) (2 * Math.PI);
        public const float Deg2Rad = PI / 180f;
        public const float Rad2Deg = 180f / PI;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Rotate(float x, float y, float sin, float cos)
        {
            return new float2
            {
                x = x * cos - y * sin,
                y = x * sin + y * cos
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Rotate(float x, float angle)
        {
            SinCos(angle, out var sin, out var cos);
            return new float2(x * cos, x * sin);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Rotate(float2 vector, float angle)
        {
            SinCos(angle, out var sin, out var cos);
            return Rotate(vector.x, vector.y, sin, cos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            
            if (value > max)
            {
                return max;
            }

            return value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(float value)
        {
            if (value < 0)
            {
                return 0;
            }

            if (value > 1)
            {
                return 1;
            }

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude(float2 vector)
        {
            return (float) Math.Sqrt(vector.x * vector.x + vector.y * vector.y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Repeat(float value, float length)
        {
            return Clamp(value - (float) Math.Floor(value / length) * length, 0, length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DeltaAngle(float currentRad, float targetRad)
        {
            var delta = Repeat(targetRad - currentRad, TwoPI);
            if (delta > PI)
            {
                return delta - TwoPI;
            }

            return delta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            var delta = target - current;
            if (Math.Abs(delta) <= maxDelta)
            {
                return target;
            }

            return current + Math.Sign(delta) * maxDelta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 MoveTowards(float2 current, float2 target, float maxDelta)
        {
            var deltaX = target.x - current.x;
            var deltaY = target.y - current.y;
            var distanceSqr = deltaX * deltaX + deltaY * deltaY;
            if (distanceSqr == 0 || maxDelta >= 0 && distanceSqr < maxDelta * maxDelta)
            {
                return target;
            }

            var distance = (float) Math.Sqrt(distanceSqr);
            
            return new float2
            {
                x = current.x + deltaX / distance * maxDelta,
                y = current.y + deltaY / distance * maxDelta
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MoveAngleTowards(float currentRad, float targetRad, float maxDeltaRad)
        {
            var delta = DeltaAngle(currentRad, targetRad);
            if (-maxDeltaRad < delta && delta < maxDeltaRad)
            {
                return targetRad;
            }

            targetRad = currentRad + delta;
            
            return MoveTowards(currentRad, targetRad, maxDeltaRad);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Atan2(float2 vector)
        {
            var angle = (float) Math.Atan2(vector.y, vector.x);
            if (angle < 0)
            {
                return TwoPI + angle;
            }
            
            return angle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(float2 a, float2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Normalize(float2 vector)
        {
            return math.normalize(vector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Rotate(float2 vector, float sin, float cos)
        {
            return Rotate(vector.x, vector.y, sin, cos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpAngle(float a, float b, float t)
        {
            var num = Repeat(b - a, TwoPI);
            if (num > PI)
            {
                num -= TwoPI;
            }

            return a + num * Clamp01(t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SinCos(float angleRad, out float sin, out float cos)
        {
            sin = (float) Math.Sin(angleRad);
            cos = (float) Math.Cos(angleRad);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MinMax(float a, float b, out float min, out float max)
        {
            if (a > b)
            {
                min = b;
                max = a;
            }
            else
            {
                min = a;
                max = b;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MinMax(float a, float b, float c, float d, out float min, out float max)
        {
            MinMax(a, b, out var min0, out var max0);
            MinMax(c, d, out var min1, out var max1);
            min = min0 < min1 ? min0 : min1;
            max = max0 > max1 ? max0 : max1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(float2 pos0, float2 pos1)
        {
            return math.distance(pos0, pos1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BoundsOverlap(float min0, float max0, float min1, float max1)
        {
            return (max1 >= min0) && (max0 >= min1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool BoundsOverlap(FloatBounds a, FloatBounds b)
        {
            if (!BoundsOverlap(a.xMin, a.xMax, b.xMin, b.xMax))
            {
                return false;
            }

            return BoundsOverlap(a.yMin, a.yMax, b.yMin, b.yMax);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 GetBoundsCenter(FloatBounds bounds)
        {
            return new float2
            {
                x = (bounds.xMin + bounds.xMax) / 2f,
                y = (bounds.yMin + bounds.yMax) / 2f
            };
        }
    }
}