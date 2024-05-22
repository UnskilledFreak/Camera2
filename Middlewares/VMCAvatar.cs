using Camera2.Interfaces;
using Camera2.Enums;
using Camera2.VMC;
using UnityEngine;

namespace Camera2.Middlewares
{
    internal class VmcAvatar : CamMiddleware, IMHandler
    {
        private static OscClient sender;

        private float _prevFov;
        private Vector3 _prevPos;
        private Quaternion _prevRot;

        public new void Post()
        {
            if (Cam.Settings.VmcProtocol.Mode == VmcMode.Disabled)
            {
                return;
            }

            if (_prevFov == Cam.Settings.FOV && _prevPos == Cam.TransformChain.Position && _prevRot == Cam.TransformChain.Rotation)
            {
                return;
            }

            try
            {
                sender ??= new OscClient(Cam.Settings.VmcProtocol.Address);

                sender.SendCamPos(Cam);
            } catch
            {
                // ignored
            } finally
            {
                _prevFov = Cam.Settings.FOV;
                _prevPos = Cam.TransformChain.Position;
                _prevRot = Cam.TransformChain.Rotation;
            }
        }
    }
}