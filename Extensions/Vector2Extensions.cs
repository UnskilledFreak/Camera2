using UnityEngine;

namespace Camera2.Extensions
{
    internal static class Vector2Extensions
    {
        internal static float InBoundary(this Vector2 boundary, float value)
        {
            if (!float.IsNegativeInfinity(boundary.x) || !float.IsPositiveInfinity(boundary.y))
            {
                value = Mathf.Clamp(value, boundary.x, boundary.y);
            }

            return value;
        }
    }
}