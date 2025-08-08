using Camera2.Interfaces;
using Camera2.Enums;
using Camera2.VMC;
using UnityEngine;

namespace Camera2.Middlewares
{
    internal class VmcAvatarMiddleware : CamMiddleware, IMHandler
    {
        private static OscClient _sender;

        private static float _prevFov;
        private static Vector3 _prevPos;
        private static Quaternion _prevRot;

        public bool Pre() => true;

        public void Post()
        {
            if (Cam.Settings.VmcProtocol.Mode == VmcMode.Disabled)
            {
                return;
            }

            if (_prevFov == Cam.Settings.FOV && _prevPos == Chain.Position && _prevRot == Chain.Rotation)
            {
                return;
            }

            try
            {
                _sender ??= new OscClient(Cam.Settings.VmcProtocol.Address);

                _sender.SendCamPos(Cam);
            }
            catch
            {
                // ignored
            }
            finally
            {
                _prevFov = Cam.Settings.FOV;
                _prevPos = Chain.Position;
                _prevRot = Chain.Rotation;
            }
        }

        public void CamConfigReloaded() { }
        
        public void ForceReset() { }
    }
}