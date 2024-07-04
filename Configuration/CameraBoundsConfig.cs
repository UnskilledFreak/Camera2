using System;
using System.Globalization;
using Camera2.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace Camera2.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class CameraBoundsConfig
    {
        private static readonly IFormatProvider Formater = CultureInfo.InvariantCulture.NumberFormat;
        
        public Bounds PosBounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));
        public Bounds RotBounds = new Bounds(Vector3.zero, new Vector3(360, 360, 360));

        [JsonProperty("pos_x")]
        public string PosX
        {
            get => string.Format(Formater, "{0}:{1}", PosBounds.min.x, PosBounds.max.x);
            set
            {
                Parse(value, false, out var min, out var max);
                PosBounds.min = new Vector3(min, PosBounds.min.y, PosBounds.min.z);
                PosBounds.max = new Vector3(max, PosBounds.max.y, PosBounds.max.z);
            }
        }

        [JsonProperty("pos_y")]
        public string PosY
        {
            get => string.Format(Formater, "{0}:{1}", PosBounds.min.y, PosBounds.max.y);
            set
            {
                Parse(value, false, out var min, out var max);
                PosBounds.min = new Vector3(PosBounds.min.x, min, PosBounds.min.z);
                PosBounds.max = new Vector3(PosBounds.max.x, max, PosBounds.max.z);
            }
        }

        [JsonProperty("pos_z")]
        public string PosZ
        {
            get => string.Format(Formater, "{0}:{1}", PosBounds.min.z, PosBounds.max.z);
            set
            {
                Parse(value, false, out var min, out var max);
                PosBounds.min = new Vector3(PosBounds.min.x, PosBounds.min.y, min);
                PosBounds.max = new Vector3(PosBounds.max.x, PosBounds.max.y, max);
            }
        }

        [JsonProperty("rot_x")]
        public string RotX
        {
            get => string.Format(Formater, "{0}:{1}", RotBounds.min.x, RotBounds.max.x);
            set
            {
                Parse(value, true, out var min, out var max);
                RotBounds.min = new Vector3(min, RotBounds.min.y, RotBounds.min.z);
                RotBounds.max = new Vector3(max, RotBounds.max.y, RotBounds.max.z);
            }
        }

        [JsonProperty("rot_y")]
        public string RotY
        {
            get => string.Format(Formater, "{0}:{1}", RotBounds.min.y, RotBounds.max.y);
            set
            {
                Parse(value, true, out var min, out var max);
                RotBounds.min = new Vector3(RotBounds.min.x, min, RotBounds.min.z);
                RotBounds.max = new Vector3(RotBounds.max.x, max, RotBounds.max.z);
            }
        }

        [JsonProperty("rot_z")]
        public string RotZ
        {
            get => string.Format(Formater, "{0}:{1}", RotBounds.min.z, RotBounds.max.z);
            set
            {
                Parse(value, true, out var min, out var max);
                RotBounds.min = new Vector3(RotBounds.min.x, RotBounds.min.y, min);
                RotBounds.max = new Vector3(RotBounds.max.x, RotBounds.max.y, max);
            }
        }

        private void Parse(string val, bool limitRot, out float min, out float max)
        {
            min = limitRot ? 0 : -500;
            max = limitRot ? 360 : 500;

            if (string.IsNullOrEmpty(val))
            {
                return;
            }

            var spl = val.Split(':');
            
            min = spl[0].SaveParseToFloat(limitRot ? 0 : -500, Formater);
            max = (spl.Length > 1 ? spl[1] : "Infinite").SaveParseToFloat(limitRot ? 360 : 500, Formater);
        }
    }
}