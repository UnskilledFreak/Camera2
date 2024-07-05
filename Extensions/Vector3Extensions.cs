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
    }
}