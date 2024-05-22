using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Camera2.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class CameraBoundsConfig
    {
        private static readonly IFormatProvider Formater = CultureInfo.InvariantCulture.NumberFormat;

        public float RotXMin = float.NegativeInfinity;
        public float RotXMax = float.PositiveInfinity;

        public float RotYMin = float.NegativeInfinity;
        public float RotYMax = float.PositiveInfinity;

        public float RotZMin = float.NegativeInfinity;
        public float RotZMax = float.PositiveInfinity;


        public float PosXMin = float.NegativeInfinity;
        public float PosXMax = float.PositiveInfinity;

        public float PosYMin = float.NegativeInfinity;
        public float PosYMax = float.PositiveInfinity;

        public float PosZMin = float.NegativeInfinity;
        public float PosZMax = float.PositiveInfinity;

        [JsonProperty("pos_x")]
        public string PosX
        {
            get => string.Format(Formater, "{0}:{1}", PosXMin, PosXMax);
            set => ParseInto(ref PosXMin, ref PosXMax, value);
        }

        [JsonProperty("pos_y")]
        public string PosY
        {
            get => string.Format(Formater, "{0}:{1}", PosYMin, PosYMax); 
            set => ParseInto(ref PosYMin, ref PosYMax, value);
        }

        [JsonProperty("pos_z")]
        public string PosZ
        {
            get => string.Format(Formater, "{0}:{1}", PosZMin, PosZMax); 
            set => ParseInto(ref PosZMin, ref PosZMax, value);
        }

        [JsonProperty("rot_x")]
        public string RotX
        {
            get => string.Format(Formater, "{0}:{1}", RotXMin, RotXMax); 
            set => ParseInto(ref RotXMin, ref RotXMax, value);
        }

        [JsonProperty("rot_y")]
        public string RotY
        {
            get => string.Format(Formater, "{0}:{1}", RotYMin, RotYMax); 
            set => ParseInto(ref RotYMin, ref RotYMax, value);
        }

        [JsonProperty("rot_z")]
        public string RotZ
        {
            get => string.Format(Formater, "{0}:{1}", RotZMin, RotZMax); 
            set => ParseInto(ref RotZMin, ref RotZMax, value);
        }

        private static void ParseInto(ref float min, ref float max, string val)
        {
            min = float.NegativeInfinity;
            max = float.PositiveInfinity;

            if (string.IsNullOrEmpty(val))
            {
                return;
            }

            var spl = val.Split(':');

            if (!float.TryParse(spl[0], NumberStyles.Float, Formater, out min))
            {
                max = float.PositiveInfinity;
            }

            if (spl.Length == 1 || !float.TryParse(spl[1], NumberStyles.Float, Formater, out max))
            {
                max = float.PositiveInfinity;
            }
        }
    }
}