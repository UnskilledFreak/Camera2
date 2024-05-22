using UnityEngine;

namespace Camera2.Interfaces
{
    public interface ISource
    {
        public string Name { get; }
        public bool IsInReplay { get; }
        // yikes
        public Vector3 localHeadPosition { get; }
        // yikes
        public Quaternion localHeadRotation { get; }
    }
}