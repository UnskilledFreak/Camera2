using Camera2.Interfaces;
using UnityEngine;

namespace Camera2.Middlewares
{
    class FPSLimiter : CamMiddleware, IMHandler
    {
        private float _renderTimeRollAccu;

        public new bool Pre()
        {
            if (!enabled || Settings.FPSLimiter.FPSLimit <= 0 || Application.targetFrameRate == Settings.FPSLimiter.FPSLimit)
            {
                return true;
            }

            if (Cam.TimeSinceLastRender + _renderTimeRollAccu < Settings.FPSLimiter.TargetFrameTime)
            {
                return false;
            }

            _renderTimeRollAccu = (Cam.TimeSinceLastRender + _renderTimeRollAccu) % Settings.FPSLimiter.TargetFrameTime;
            return true;
        }
    }
}