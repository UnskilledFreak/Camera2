using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Camera2.Utils
{
    internal class TransformChain
    {
        private Dictionary<string, Transformer> _transformers = new Dictionary<string, Transformer>();

        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        private readonly Transform _targetBase;
        private readonly Transform _target;

        public TransformChain(Transform targetBase, Transform target = null)
        {
            _targetBase = targetBase;
            _target = target;
        }

        public Transformer AddOrGet(string name, int order, bool sortIn = true)
        {
            if (_transformers.TryGetValue(name, out var transformer))
            {
                return transformer;
            }

            transformer = new Transformer { Order = order };

            _transformers.Add(name, transformer);

            if (sortIn)
            {
                Resort(false);
            }

            return transformer;
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

            foreach (var transformer in _transformers.Select(mapper => mapper.Value))
            {
                if (transformer.Position != Vector3.zero)
                {
                    Position += transformer.ApplyAsAbsolute ? transformer.Position : Rotation * transformer.Position;
                }

                if (transformer.Rotation == Quaternion.identity)
                {
                    continue;
                }

                if (transformer.ApplyAsAbsolute)
                {
                    Rotation = Quaternion.Inverse(Rotation) * transformer.Rotation;
                }
                else
                {
                    Rotation *= transformer.Rotation;
                }
            }

            if (_target == null || !apply)
            {
                return;
            }

            _target.position = Position;
            _target.rotation = Rotation;
        }

        private void Resort(bool calculate = true)
        {
            _transformers = _transformers.OrderBy(x => x.Value.Order)
                .ToDictionary(x => x.Key, x => x.Value);

            if (calculate)
            {
                Calculate();
            }
        }
    }
}