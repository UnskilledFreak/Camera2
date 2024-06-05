using UnityEngine;

namespace Camera2.Utils
{
    internal class Transformer
    {
        public Vector3 Position = Vector3.zero;
        public Quaternion Rotation = Quaternion.identity;
        public Vector3 RotationEuler { set => Rotation = Quaternion.Euler(value); }
        public int Order = 0;
        public bool ApplyAsAbsolute = false;
    }
}