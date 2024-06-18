using Camera2.Behaviours;
using Camera2.Configuration;
using Camera2.Interfaces;
using Camera2.Utils;
using UnityEngine;

namespace Camera2.Middlewares
{
    internal abstract class CamMiddleware : MonoBehaviour
    {
        protected Cam2 Cam;
        protected bool TeleportOnNextFrame;
        protected CameraSettings Settings => Cam.Settings;
        protected TransformChain Chain => Cam.TransformChain;
        protected Transformer Transformer;

        public IMHandler Init(Cam2 cam)
        {
            Cam = cam;
            return (IMHandler)this;
        }

        protected void AddTransformer(TransformerTypeAndOrder type, bool asAbsolute = false)
        {
            Transformer = Chain.AddOrGet(type);
            Transformer.ApplyAsAbsolute = asAbsolute;
        }

        protected void RemoveTransformer(TransformerTypeAndOrder type)
        {
            Chain.Remove(type);
            Transformer = null;
        }
    }
}