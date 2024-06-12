using UnityEngine;

namespace Camera2.Extensions
{
    internal static class Vector3Extensions
    {
        internal static void InBoundary(this Vector3 vector3, Vector2 boundaryX, Vector2 boundaryY, Vector2 boundaryZ)
        {
            vector3.x = boundaryX.InBoundary(vector3.x);
            vector3.y = boundaryY.InBoundary(vector3.y);
            vector3.z = boundaryZ.InBoundary(vector3.z);
        }
    }
}