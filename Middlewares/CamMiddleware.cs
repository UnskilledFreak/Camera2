using Camera2.Behaviours;
using Camera2.Configuration;
using Camera2.Interfaces;
using UnityEngine;

namespace Camera2.Middlewares
{
    internal abstract class CamMiddleware : MonoBehaviour
    {
        protected Cam2 Cam;
        protected CameraSettings Settings => Cam.Settings;
        protected bool TeleportOnNextFrame;

        public IMHandler Init(Cam2 cam)
        {
            Cam = cam;
            return (IMHandler)this;
        }
    }
}