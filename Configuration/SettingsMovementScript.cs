namespace Camera2.Configuration
{
    internal class SettingsMovementScript : CameraSubSettings
    {
        public string[] ScriptList { get; set; } = new string[] { };
        public bool FromOrigin { get; set; } = true;
        public bool EnableInMenu { get; set; } = false;
    }
}