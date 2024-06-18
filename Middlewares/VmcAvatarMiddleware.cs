using Camera2.Interfaces;
using Camera2.Enums;
using Camera2.VMC;
using UnityEngine;

namespace Camera2.Middlewares
{
    internal class VmcAvatarMiddleware : CamMiddleware, IMHandler
    {
        private static OscClient sender;

        private float _prevFov;
        private Vector3 _prevPos;
        private Quaternion _prevRot;

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
                sender ??= new OscClient(Cam.Settings.VmcProtocol.Address);

                sender.SendCamPos(Cam);
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
    }
}