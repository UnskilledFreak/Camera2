using Camera2.Enums;
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
                TeleportOnNextFrame = true;
                
                _transformer = Cam.TransformChain.AddOrGet("Follower", TransformerOrders.Follower);
                _transformer.ApplyAsAbsolute = true;
            }

            // use the real position, no offset here
            if (_childTransform == null)
            {
                _childTransform = Cam.Camera.transform;
            }

            var targetPosition = -(_childTransform.localPosition - Settings.Parent.position);
            if (Settings.SmoothFollow.FollowerUseOffsetRotationAsPosition)
            {
                Vector3 direction;
                switch (Settings.SmoothFollow.FollowerOffsetPositionRelativeType)
                {
                    default:
                    case FollowerPositionOffsetType.Forward:
                        direction = Settings.Parent.forward;
                        break;
                    case FollowerPositionOffsetType.Right:
                        direction = Settings.Parent.right;
                        break;
                    case FollowerPositionOffsetType.Up:
                        direction = Settings.Parent.up;
                        break;
                }
                targetPosition += Settings.SmoothFollow.FollowerOffsetPositionIsRelative 
                    ? Vector3.Scale(direction, Settings.TargetRot) 
                    : Settings.TargetRot;
            }

            var lookRotation = Quaternion.LookRotation(targetPosition);
            if (!Settings.SmoothFollow.FollowerUseOffsetRotationAsPosition && !Settings.SmoothFollow.FollowerOffsetPositionIsRelative)
            {
                lookRotation *= Quaternion.Inverse(Quaternion.Euler(Settings.TargetRot));
            }

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
            Settings.ParentChange();
            _childTransform = null;
        }
    }
}