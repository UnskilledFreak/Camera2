using Camera2.Enums;

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
        
        private bool _pivotingOffset = true;
        public string TargetParent { get; set; } = "";
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