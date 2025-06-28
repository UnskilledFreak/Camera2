using UnityEngine;

namespace Camera2.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3 ClampToMiddlePointAngles(this Vector3 eulerAngles)
        {
            return new Vector3(
                eulerAngles.x.GetNiceRotationNumber(),
                eulerAngles.y.GetNiceRotationNumber(),
                eulerAngles.z.GetNiceRotationNumber()
            );
        }

        public static Vector3 Clone(this Vector3 vector) => new(vector.x, vector.y, vector.z);

        // have to do it manually because bounds.Contains() does not work with negative values????
        public static Vector3 ClampToBoundary(this Vector3 vector, Bounds bounds) => new(
            Mathf.Clamp(vector.x, bounds.min.x, bounds.max.x),
            Mathf.Clamp(vector.y, bounds.min.y, bounds.max.y),
            Mathf.Clamp(vector.z, bounds.min.z, bounds.max.z)
        );

        public static Vector3 Slerp(this Vector3 a, Vector3 b, float t) => Vector3.Slerp(a, b, t);
    }
}