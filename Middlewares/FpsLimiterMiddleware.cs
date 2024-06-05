using Camera2.Interfaces;
using UnityEngine;

namespace Camera2.Middlewares
{
    internal class FpsLimiterMiddleware : CamMiddleware, IMHandler
    {
        private float _renderTimeRollAcc;

        public bool Pre()
        {
            if (!enabled || Settings.FPSLimiter.FPSLimit <= 0 || Application.targetFrameRate == Settings.FPSLimiter.FPSLimit)
            {
                return true;
            }

            if (Cam.TimeSinceLastRender + _renderTimeRollAcc < Settings.FPSLimiter.TargetFrameTime)
            {
                return false;
            }

            _renderTimeRollAcc = (Cam.TimeSinceLastRender + _renderTimeRollAcc) % Settings.FPSLimiter.TargetFrameTime;
            return true;
        }

        public void Post() { }

        public void CamConfigReloaded() { }
    }
}