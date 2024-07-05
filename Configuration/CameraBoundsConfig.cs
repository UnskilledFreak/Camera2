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
        public static Vector2 PositionBoundary = new Vector2(-500, 500);
        public static Vector2 RotationBoundary = new Vector2(-180, 180);

        public Bounds PositionBounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000));
        public Bounds RotationBounds = new Bounds(Vector3.zero, new Vector3(360, 360, 360));

        private void SetBounds(ref Bounds bounds, string value, bool limitRot, bool valueIsY = false, bool valueIsZ = false)
        {
            Parse(value, limitRot ? RotationBoundary : PositionBoundary, out var min, out var max);
            
            bounds.min = new Vector3(
                valueIsY || valueIsZ ? bounds.min.x : min,
                valueIsY ? min : bounds.min.y,
                valueIsZ ? min : bounds.min.z
            );
            bounds.max = new Vector3(
                valueIsY || valueIsZ ? bounds.max.x : max,
                valueIsY ? max : bounds.max.y,
                valueIsZ ? max : bounds.max.z
            );
        }

        [JsonProperty("pos_x")]
        public string PositionX
        {
            get => string.Format(Formater, "{0}:{1}", PositionBounds.min.x, PositionBounds.max.x);
            set => SetBounds(ref PositionBounds, value, false);
        }

        [JsonProperty("pos_y")]
        public string PositionY
        {
            get => string.Format(Formater, "{0}:{1}", PositionBounds.min.y, PositionBounds.max.y);
            set => SetBounds(ref PositionBounds, value, false, true);
        }

        [JsonProperty("pos_z")]
        public string PositionZ
        {
            get => string.Format(Formater, "{0}:{1}", PositionBounds.min.z, PositionBounds.max.z);
            set => SetBounds(ref PositionBounds, value, false, valueIsZ: true);
        }

        [JsonProperty("rot_x")]
        public string RotationX
        {
            get => string.Format(Formater, "{0}:{1}", RotationBounds.min.x, RotationBounds.max.x);
            set => SetBounds(ref RotationBounds, value, true);
        }

        [JsonProperty("rot_y")]
        public string RotationY
        {
            get => string.Format(Formater, "{0}:{1}", RotationBounds.min.y, RotationBounds.max.y);
            set => SetBounds(ref RotationBounds, value, true, true);
        }

        [JsonProperty("rot_z")]
        public string RotationZ
        {
            get => string.Format(Formater, "{0}:{1}", RotationBounds.min.z, RotationBounds.max.z);
            set => SetBounds(ref RotationBounds, value, true, valueIsZ: true);
        }

        private void Parse(string val, Vector2 boundary, out float min, out float max)
        {
            min = boundary.x;
            max = boundary.y;

            if (string.IsNullOrEmpty(val))
            {
                return;
            }

            var spl = val.Split(':');

            min = spl[0].SaveParseToFloat(boundary.x, Formater, boundary);
            max = (spl.Length > 1 ? spl[1] : "Infinite").SaveParseToFloat(boundary.y, Formater, boundary);
        }
    }
}