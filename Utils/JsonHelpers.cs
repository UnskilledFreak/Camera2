using System;
using Newtonsoft.Json;

namespace Camera2.Utils
{
    internal static class JsonHelpers
    {
        public static float LimitFloatResolution(float val) => (float)Math.Round(val, 4);

        public static readonly JsonSerializerSettings LeanDeserializeSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Error = (se, ev) =>
            {
#if DEBUG
                Plugin.Log.Warn("Failed JSON deserialize:");
                Plugin.Log.Warn(ev.ErrorContext.Error);
#endif
                ev.ErrorContext.Handled = true;
            }
        };
    }
}