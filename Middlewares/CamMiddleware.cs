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

        public IMHandler Init(Cam2 cam)
        {
            Cam = cam;
            return (IMHandler)this;
        }
    }
}