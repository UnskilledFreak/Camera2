﻿using Camera2.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Camera2.Configuration
{
    internal class SettingsSmoothFollow : CameraSubSettings
    {
        public float Position = 10f;
        public float Rotation = 4f;
        public bool FollowReplayPosition = true;
        //public bool FollowerDistanceEnabled = true;
        //public float FollowerDistance = 3f;
        
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

        [JsonIgnore] internal bool UseLocalPosition = true;
        [JsonIgnore] internal Transform Parent;
        [JsonIgnore] internal Transformer Transformer;
    }
}