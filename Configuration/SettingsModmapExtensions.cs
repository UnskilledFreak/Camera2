using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.Configuration
{
    internal class SettingsModmapExtensions : CameraSubSettings {
        public bool MoveWithMap = true;
        public bool AutoOpaqueWalls = false;
        public bool AutoHideHUD = false;

        private float _parentLookupDelay = 0.1f;

        public float ParentLookupDelay
        {
            get => _parentLookupDelay;
            set => _parentLookupDelay = Mathf.Clamp(value, CameraSettings.MinParentLookupDelay, CameraSettings.MaxParentLookupDelay);
        }

        // some Newtonsoft magic
        [UsedImplicitly]
        public bool ShouldSerializeMoveWithMap() => Settings.IsPositionalCam();
    }
}