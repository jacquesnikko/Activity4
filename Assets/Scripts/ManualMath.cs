using System;
using UnityEngine;

namespace AppliedMath.Week3
{
    /// <summary>
    /// Small 2D vector used by the gameplay code. Unity's Vector2 math helpers are
    /// intentionally avoided so the important calculations remain visible.
    /// </summary>
    [Serializable]
    public struct GameVector2
    {
        public float x;
        public float y;

        public GameVector2(float xValue, float yValue)
        {
            x = xValue;
            y = yValue;
        }

        public static GameVector2 zero => new GameVector2(0f, 0f);

        public static GameVector2 operator +(GameVector2 a, GameVector2 b) =>
            new GameVector2(a.x + b.x, a.y + b.y);

        public static GameVector2 operator -(GameVector2 a, GameVector2 b) =>
            new GameVector2(a.x - b.x, a.y - b.y);

        public static GameVector2 operator *(GameVector2 value, float scalar) =>
            new GameVector2(value.x * scalar, value.y * scalar);

        public static GameVector2 operator *(float scalar, GameVector2 value) => value * scalar;

        public static GameVector2 operator /(GameVector2 value, float scalar) =>
            new GameVector2(value.x / scalar, value.y / scalar);

        public Vector3 ToVector3(float z = 0f) => new Vector3(x, y, z);

        public static GameVector2 FromUnity(Vector3 value) => new GameVector2(value.x, value.y);
    }

    [Serializable]
    public struct GameAabb
    {
        public GameVector2 center;
        public GameVector2 halfSize;

        public GameAabb(GameVector2 centerValue, GameVector2 halfSizeValue)
        {
            center = centerValue;
            halfSize = halfSizeValue;
        }

        public float MinX => center.x - halfSize.x;
        public float MaxX => center.x + halfSize.x;
        public float MinY => center.y - halfSize.y;
        public float MaxY => center.y + halfSize.y;
    }

    public static class ManualMath
    {
        public const float Epsilon = 0.000001f;
        public const float DegreesToRadians = (float)(Math.PI / 180.0);
        public const float RadiansToDegrees = (float)(180.0 / Math.PI);

        public static float Clamp(float value, float minimum, float maximum)
        {
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }

        public static float Clamp01(float value) => Clamp(value, 0f, 1f);

        public static float Lerp(float start, float end, float t) => start + (end - start) * t;

        public static GameVector2 Lerp(GameVector2 start, GameVector2 end, float t) =>
            start + (end - start) * t;

        public static float Dot(GameVector2 a, GameVector2 b) => a.x * b.x + a.y * b.y;

        public static float LengthSquared(GameVector2 value) => Dot(value, value);

        public static float Length(GameVector2 value) => (float)Math.Sqrt(LengthSquared(value));

        public static GameVector2 NormalizeSafe(GameVector2 value)
        {
            float lengthSquared = LengthSquared(value);
            if (lengthSquared <= Epsilon)
            {
                return zeroVector;
            }

            float inverseLength = 1f / (float)Math.Sqrt(lengthSquared);
            return value * inverseLength;
        }

        private static readonly GameVector2 zeroVector = new GameVector2(0f, 0f);

        public static float DistanceSquared(GameVector2 a, GameVector2 b) =>
            LengthSquared(a - b);

        public static GameVector2 DirectionFromDegrees(float angleDegrees)
        {
            double radians = angleDegrees * DegreesToRadians;
            return new GameVector2((float)Math.Cos(radians), (float)Math.Sin(radians));
        }

        public static float AngleDegrees(GameVector2 direction)
        {
            return (float)Math.Atan2(direction.y, direction.x) * RadiansToDegrees;
        }

        public static float Repeat(float value, float length)
        {
            return value - (float)Math.Floor(value / length) * length;
        }

        public static float DeltaAngleDegrees(float current, float target)
        {
            float delta = Repeat(target - current + 180f, 360f) - 180f;
            return delta;
        }

        public static float CosDegrees(float degrees) =>
            (float)Math.Cos(degrees * DegreesToRadians);

        public static bool IsInsideCone(
            GameVector2 origin,
            GameVector2 forward,
            GameVector2 target,
            float range,
            float fullConeAngleDegrees)
        {
            GameVector2 toTarget = target - origin;
            float distanceSquared = LengthSquared(toTarget);
            if (distanceSquared > range * range || distanceSquared <= Epsilon)
            {
                return false;
            }

            GameVector2 direction = NormalizeSafe(toTarget);
            GameVector2 unitForward = NormalizeSafe(forward);
            float alignment = Dot(unitForward, direction);
            float minimumAlignment = CosDegrees(fullConeAngleDegrees * 0.5f);
            return alignment >= minimumAlignment;
        }

        public static bool IsInsideSightLine(
            GameVector2 origin,
            GameVector2 forward,
            GameVector2 target,
            float range,
            float halfWidth)
        {
            GameVector2 unitForward = NormalizeSafe(forward);
            GameVector2 toTarget = target - origin;
            float alongLine = Dot(toTarget, unitForward);
            if (alongLine < 0f || alongLine > range)
            {
                return false;
            }

            GameVector2 perpendicular = new GameVector2(-unitForward.y, unitForward.x);
            float sidewaysDistance = Math.Abs(Dot(toTarget, perpendicular));
            return sidewaysDistance <= halfWidth;
        }

        public static bool SegmentIntersectsCircle(
            GameVector2 segmentStart,
            GameVector2 segmentEnd,
            GameVector2 circleCenter,
            float circleRadius)
        {
            GameVector2 segment = segmentEnd - segmentStart;
            float segmentLengthSquared = LengthSquared(segment);
            float t = 0f;

            if (segmentLengthSquared > Epsilon)
            {
                t = Dot(circleCenter - segmentStart, segment) / segmentLengthSquared;
                t = Clamp01(t);
            }

            GameVector2 closestPoint = segmentStart + segment * t;
            return DistanceSquared(closestPoint, circleCenter) <= circleRadius * circleRadius;
        }

        public static bool CircleOverlapsAabb(GameVector2 center, float radius, GameAabb box)
        {
            float closestX = Clamp(center.x, box.MinX, box.MaxX);
            float closestY = Clamp(center.y, box.MinY, box.MaxY);
            float deltaX = center.x - closestX;
            float deltaY = center.y - closestY;
            return deltaX * deltaX + deltaY * deltaY < radius * radius;
        }

        public static bool SegmentIntersectsAabb(
            GameVector2 start,
            GameVector2 end,
            GameAabb box,
            float padding)
        {
            float minX = box.MinX - padding;
            float maxX = box.MaxX + padding;
            float minY = box.MinY - padding;
            float maxY = box.MaxY + padding;
            GameVector2 direction = end - start;
            float tMinimum = 0f;
            float tMaximum = 1f;

            if (!ClipAxis(start.x, direction.x, minX, maxX, ref tMinimum, ref tMaximum))
                return false;

            return ClipAxis(start.y, direction.y, minY, maxY, ref tMinimum, ref tMaximum);
        }

        private static bool ClipAxis(
            float origin,
            float direction,
            float minimum,
            float maximum,
            ref float tMinimum,
            ref float tMaximum)
        {
            if (Math.Abs(direction) <= Epsilon)
            {
                return origin >= minimum && origin <= maximum;
            }

            float inverse = 1f / direction;
            float near = (minimum - origin) * inverse;
            float far = (maximum - origin) * inverse;
            if (near > far)
            {
                float temporary = near;
                near = far;
                far = temporary;
            }

            if (near > tMinimum) tMinimum = near;
            if (far < tMaximum) tMaximum = far;
            return tMinimum <= tMaximum;
        }
    }
}
