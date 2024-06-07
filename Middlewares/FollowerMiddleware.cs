using Camera2.Interfaces;
using Camera2.Utils;
using UnityEngine;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Middlewares
{
    internal class FollowerMiddleware : CamMiddleware, IMHandler
    {
        private Transformer _transformer;
        private Transform _childTransform;

        public void OnEnable()
        {
            TeleportOnNextFrame = true;
        }

        public bool Pre()
        {
            if (Settings.Type != CameraType.Follower || Settings.Parent == null)
            {
                return true;
            }

            if (_transformer == null)
            {
                Settings.SmoothFollow.UseLocalPosition = false;
                TeleportOnNextFrame = true;
                
                _transformer = Cam.TransformChain.AddOrGet("Follower", TransformerOrders.Follower);
                _transformer.ApplyAsAbsolute = true;
            }

            // use the real position, no offset here
            if (_childTransform == null)
            {
                _childTransform = Cam.Camera.transform;
            }

            // have to negate direction because cams are somehow flipped
            var direction = -(_childTransform.localPosition - Settings.Parent.position);
            var lookRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(Settings.TargetRot);
            
            _transformer.Position = Vector3.zero;

            if (TeleportOnNextFrame)
            {
                _transformer.Rotation = Quaternion.identity;
                TeleportOnNextFrame = false;
            }
            else
            {
                _transformer.Rotation = Quaternion.Slerp(_transformer.Rotation, lookRotation, Cam.TimeSinceLastRender * Settings.SmoothFollow.Rotation);
            }

            return true;
        }

        public void Post() { }

        public void CamConfigReloaded()
        {
            _childTransform = null;
        }
    }
}