using System;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace Camera2.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class CameraBoundsConfig
    {
        private static readonly IFormatProvider Formater = CultureInfo.InvariantCulture.NumberFormat;

        public Vector2 PosVectorX = new Vector2(float.NegativeInfinity, float.PositiveInfinity);
        public Vector2 PosVectorY = new Vector2(float.NegativeInfinity, float.PositiveInfinity);
        public Vector2 PosVectorZ = new Vector2(float.NegativeInfinity, float.PositiveInfinity);

        public Vector2 RotVectorX = new Vector2(float.NegativeInfinity, float.PositiveInfinity);
        public Vector2 RotVectorY = new Vector2(float.NegativeInfinity, float.PositiveInfinity);
        public Vector2 RotVectorZ = new Vector2(float.NegativeInfinity, float.PositiveInfinity);

        [JsonProperty("pos_x")]
        public string PosX
        {
            get => string.Format(Formater, "{0}:{1}", PosVectorX.x, PosVectorX.y);
            set => ParseInto(PosVectorX, value);
        }

        [JsonProperty("pos_y")]
        public string PosY
        {
            get => string.Format(Formater, "{0}:{1}", PosVectorY.x, PosVectorY.y);
            set => ParseInto(PosVectorY, value);
        }

        [JsonProperty("pos_z")]
        public string PosZ
        {
            get => string.Format(Formater, "{0}:{1}", PosVectorZ.x, PosVectorZ.y);
            set => ParseInto(PosVectorZ, value);
        }

        [JsonProperty("rot_x")]
        public string RotX
        {
            get => string.Format(Formater, "{0}:{1}", RotVectorX.x, RotVectorX.y);
            set => ParseInto(RotVectorX, value);
        }

        [JsonProperty("rot_y")]
        public string RotY
        {
            get => string.Format(Formater, "{0}:{1}", RotVectorY.x, RotVectorY.y);
            set => ParseInto(RotVectorX, value);
        }

        [JsonProperty("rot_z")]
        public string RotZ
        {
            get => string.Format(Formater, "{0}:{1}", RotVectorZ.x, RotVectorZ.y);
            set => ParseInto(RotVectorX, value);
        }

        private static void ParseInto(Vector2 vector2, string val)
        {
            vector2.x = float.NegativeInfinity;
            vector2.y = float.PositiveInfinity;

            if (string.IsNullOrEmpty(val))
            {
                return;
            }

            var spl = val.Split(':');

            if (!float.TryParse(spl[0], NumberStyles.Float, Formater, out vector2.x))
            {
                vector2.x = float.PositiveInfinity;
            }

            if (spl.Length == 1 || !float.TryParse(spl[1], NumberStyles.Float, Formater, out vector2.y))
            {
                vector2.y = float.PositiveInfinity;
            }
        }
    }
}