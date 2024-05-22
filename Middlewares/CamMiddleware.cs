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

        public IMHandler Init(Cam2 cam)
        {
            Cam = cam;
            return (IMHandler)this;
        }

        // Prevents the cam from rendering this frame if returned false
        public bool Pre() { return true; }
        public void Post() { }
        public void CamConfigReloaded() { }
    }
}