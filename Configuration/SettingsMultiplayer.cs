using Newtonsoft.Json;

namespace Camera2.Configuration
{
    internal class SettingsMultiplayer : CameraSubSettings
    {
        [JsonProperty("followSpectatorPlattform")]
        public bool FollowSpectatorPlatform = true;
    }
}