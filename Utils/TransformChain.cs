using System.Collections.Generic;
using System.Linq;
using Camera2.Behaviours;
using UnityEngine;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Utils
{
    internal class TransformChain
    {
        private Dictionary<TransformerTypeAndOrder, Transformer> _transformers = new Dictionary<TransformerTypeAndOrder, Transformer>();

        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }

        private readonly Transform _targetBase;
        private readonly Transform _target;
        private readonly Cam2 _cam;

        public TransformChain(Cam2 cam)
        {
            _cam = cam;
            _targetBase = _cam.transform;
            _target = _cam.Camera.transform;
        }

        public string DebugChain()
        {
            return "Transformer Chain: " + string.Join(", ", _transformers.OrderBy(x => x.Value.Order).Select(x => $"{x.Key} @ {x.Value.Order}"));
        }
        
        public bool HasType(TransformerTypeAndOrder type) => _transformers.ContainsKey(type);

        public Transformer AddOrGet(TransformerTypeAndOrder type, bool sortIn = true)
        {
            if (_transformers.TryGetValue(type, out var transformer))
            {
                return transformer;
            }

            transformer = new Transformer { Order = (int)type };

            _transformers.Add(type, transformer);

            if (sortIn)
            {
                Resort(false);
            }

            return transformer;
        }

        public void Remove(TransformerTypeAndOrder type)
        {
            if (HasType(type))
            {
                _transformers.Remove(type);
            }
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
                    Rotation = transformer.Rotation * Rotation;
                }
                else
                {
                    if ((TransformerTypeAndOrder)transformer.Order == TransformerTypeAndOrder.ModMapParenting && _cam.Settings.Type == CameraType.Follower)
                    {
                        // why am I to stupid to negate this?
                    }
                    else
                    {
                        Rotation *= transformer.Rotation;
                    }
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