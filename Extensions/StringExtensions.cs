using System;
using System.Globalization;

namespace Camera2.Extensions
{
    public static class StringExtensions
    {
        public static float SaveParseToFloat(this string input, float def, IFormatProvider formater)
        {
            return float.TryParse(input, NumberStyles.Float, formater, out var parse)
                ? float.IsNegativeInfinity(parse) || float.IsPositiveInfinity(parse) 
                    ? def 
                    : parse
                : def;
        }
    }
}