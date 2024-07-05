using UnityEngine;

namespace Camera2.Extensions
{
    public static class FloatExtensions
    {
        public static float GetNiceRotationNumber(this float input)
        {
            return Mathf.Clamp(
                input > 180f
                    ? input - 360f
                    : input,
                -179.99f,
                180f
            );
        }

        public static float SetFromNiceRotationNumber(this float input)
        {
            return Mathf.Clamp(input < 0f ? input + 360f : input, 0f, 359.99f);
        }
    }
}