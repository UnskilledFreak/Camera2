using Camera2.HarmonyPatches;
using Newtonsoft.Json;

namespace Camera2.Configuration
{
    internal class SettingsFPSLimiter : CameraSubSettings
    {
        private int _limit;
        [JsonIgnore] public float TargetFrameTime { get; private set; } = 1f / 60f;

        public int FPSLimit
        {
            get => _limit;
            set
            {
                _limit = value;
                TargetFrameTime = value != 0f ? 1f / value : 0f;

                GlobalFPSCap.ApplyFPSCap();
            }
        }
    }
}