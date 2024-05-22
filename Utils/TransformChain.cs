using System.Collections.Generic;
using UnityEngine;

namespace Camera2.Utils
{
    internal class TransformChain
    {
        private readonly List<Transformer> _transformers = new List<Transformer>();
        private readonly Dictionary<string, Transformer> _transformerMap = new Dictionary<string, Transformer>();

        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        private readonly Transform _targetBase;
        private readonly Transform _target;

        private void Resort(bool calculate = true)
        {
            _transformers.Sort((a, b) => a.Order - b.Order);

            if (calculate)
            {
                Calculate();
            }
        }

        public TransformChain(Transform targetBase, Transform target = null)
        {
            _targetBase = targetBase;
            _target = target;
        }

        public Transformer AddOrGet(string name, int order = 0, bool sortIn = true)
        {
            if (_transformerMap.TryGetValue(name, out var t))
            {
                return t;
            }

            t = new Transformer { Order = order };

            _transformers.Add(t);
            _transformerMap.Add(name, t);

            if (sortIn)
            {
                Resort(false);
            }

            return t;
        }
        
        public void Calculate(bool apply = true)
        {
            if (_transformers.Count == 0)
            {
                Position = Vector3.zero;
                Rotation = Quaternion.identity;
                return;
            }

            Position = _targetBase.position;
            Rotation = _targetBase.rotation;

            for (var i = 0; i != _transformers.Count; i++)
            {
                var x = _transformers[i];

                if (x.Position != Vector3.zero)
                {
                    Position += x.ApplyAsAbsolute ? x.Position : Rotation * x.Position;
                }

                if (x.Rotation != Quaternion.identity)
                {
                    if (!x.ApplyAsAbsolute)
                    {
                        Rotation *= x.Rotation;
                    } else
                    {
                        Rotation = x.Rotation * Rotation;
                    }
                }

                x.PositionSum = Position;
                x.RotationSum = Rotation;
            }

            if (_target == null || !apply)
            {
                return;
            }

            _target.position = Position;
            _target.rotation = Rotation;
        }
    }
}