using UnityEngine;

namespace Camera2.Extensions;

public static class QuaternationExtensions
{
    public static Quaternion Slerp(this Quaternion a, Quaternion b, float t) => Quaternion.Slerp(a, b, t);
}