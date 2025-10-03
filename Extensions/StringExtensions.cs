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

        public static string ToBSMLGreen(this string input, bool addIcon = true) => GetColorString("green", input, "\u2713", addIcon);
        public static string ToBSMLRed(this string input, bool addIcon = true) => GetColorString("red", input, "\u2717", addIcon);
        public static string ToBSMLYellow(this string input, bool addIcon = true) => GetColorString("#d5a145", input, "\u26A0", addIcon);
        //public static string ToBSMLYellow(this string input, bool addIcon = true) => GetColorString("#d5a145", input, "⚠", addIcon);

        private static string GetColorString(string color, string text, string icon, bool addIcon = true)
        {
            var useQuotes = !color.StartsWith("#");
            var colorCoded = useQuotes
                ? $"\"{color}\""
                : color;
            return $"<color={colorCoded}>{text}{GetIcon(icon, addIcon)}</color>";
        }

        private static string GetIcon(string icon, bool addIcon = true) => addIcon ? icon : "";
    }
}