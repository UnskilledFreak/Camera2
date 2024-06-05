using System.Collections.Generic;
using System.Linq;
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

        public TransformChain(Transform targetBase, Transform target = null)
        {
            _targetBase = targetBase;
            _target = target;
        }

        public Transformer AddOrGet(string name, int order, bool sortIn = true)
        {
            if (_transformerMap.TryGetValue(name, out var transformer))
            {
                return transformer;
            }

            transformer = new Transformer { Order = order };

            _transformers.Add(transformer);
            _transformerMap.Add(name, transformer);

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

            // this is a hack but since transformers are using offsets this won't work easily,
            // and it will save performance as well
            var followerTransformer = _transformers.FirstOrDefault(x => x.Order == TransformerOrders.Follower);
            if (followerTransformer != null)
            {
                // load positioner because otherwise grab won't work
                var positionTransformer = _transformers.FirstOrDefault(x => x.Order == TransformerOrders.PositionOffset);
                Position = positionTransformer?.Position ?? followerTransformer.Position;
                Rotation = followerTransformer.Rotation;
            }
            else
            {
                foreach (var transformer in _transformers.Where(x => x.Order != TransformerOrders.Follower))
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
            _transformers.Sort((a, b) => a.Order - b.Order);

            if (calculate)
            {
                Calculate();
            }
        }
    }
}