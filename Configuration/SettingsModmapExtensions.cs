using JetBrains.Annotations;

namespace Camera2.Configuration
{
    internal class SettingsModmapExtensions : CameraSubSettings {
        public bool MoveWithMap = true;
        public bool AutoOpaqueWalls = false;
        public bool AutoHideHUD = false;

        // some Newtonsoft magic
        [UsedImplicitly]
        public bool ShouldSerializeMoveWithMap() => Settings.IsPositionalCam();
    }
}