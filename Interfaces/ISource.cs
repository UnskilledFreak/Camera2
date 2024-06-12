using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.Interfaces
{
    public interface ISource
    {
        [UsedImplicitly]
        public string Name { get; }
        public bool IsInReplay { get; }
        // yikes, have to leave it like this so BL replay source will still work
        public Vector3 localHeadPosition { get; }
        // yikes, have to leave it like this so BL replay source will still work
        public Quaternion localHeadRotation { get; }
    }
}