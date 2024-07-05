using System;
using System.Globalization;
using UnityEngine;

namespace Camera2.Extensions
{
    public static class StringExtensions
    {
        public static float SaveParseToFloat(this string input, float def, IFormatProvider formater, Vector2 boundary)
        {
            return float.TryParse(input, NumberStyles.Float, formater, out var parse)
                ? float.IsNegativeInfinity(parse) || float.IsPositiveInfinity(parse) 
                    ? def 
                    : Mathf.Clamp(parse, boundary.x, boundary.y)
                : def;
        }
    }
}