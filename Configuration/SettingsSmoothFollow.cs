using Camera2.Enums;
using UnityEngine;

namespace Camera2.Configuration
{
    internal class SettingsSmoothFollow : CameraSubSettings
    {
        public float Position = 10f;
        public float Rotation = 4f;
        public bool FollowReplayPosition = true;
        public bool FollowerUseOffsetRotationAsPosition = false;
        public bool FollowerOffsetPositionIsRelative = false;
        public bool FollowerUseOrganic = false;
        public FollowerPositionOffsetType FollowerOffsetPositionRelativeType = FollowerPositionOffsetType.Forward;
        public FollowerFakeZoom FollowerFakeZoom = new FollowerFakeZoom();
        
        private bool _pivotingOffset = true;
        private float _targetParentLookupDelay = 0.5f;
        public string TargetParent { get; set; } = "";
        public float TargetParentLookupDelay
        {
            get => _targetParentLookupDelay;
            set => _targetParentLookupDelay = Mathf.Clamp(value, CameraSettings.MinParentLookupDelay, CameraSettings.MaxParentLookupDelay);
        }

        public bool PivotingOffset
        {
            get => _pivotingOffset;
            set
            {
                if (value == _pivotingOffset)
                {
                    return;
                }

                _pivotingOffset = value;
                Settings.Cam.Transformer.ApplyAsAbsolute = !value;
            }
        }

        public readonly CameraBoundsConfig Limits = new CameraBoundsConfig();
    }
}