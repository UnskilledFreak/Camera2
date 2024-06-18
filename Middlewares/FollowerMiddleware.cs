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
        private bool _wasInMovementScript;

        public void OnEnable()
        {
            TeleportOnNextFrame = true;
        }

        public bool Pre()
        {
            if (Settings.Type != CameraType.Follower || Settings.Parent == null)
            {
                Chain.Remove(TransformerTypeAndOrder.Follower);
                _transformer = null;
                return true;
            }

            // don't track until MovementScript is done
            if (Chain.HasType(TransformerTypeAndOrder.MovementScriptProcessor))
            {
                _wasInMovementScript = true;
                return true;
            }

            if (_transformer == null)
            {
                TeleportOnNextFrame = true;

                _transformer = Chain.AddOrGet(TransformerTypeAndOrder.Follower);
                _transformer.ApplyAsAbsolute = true;
            }

            var targetPosition = -(Cam.Camera.transform.localPosition - Settings.Parent.position);
            if (Settings.SmoothFollow.FollowerUseOffsetRotationAsPosition)
            {
                targetPosition += Settings.SmoothFollow.FollowerOffsetPositionIsRelative
                    ? Vector3.Scale(Settings.SmoothFollow.FollowerOffsetPositionRelativeType switch
                    {
                        FollowerPositionOffsetType.Right => Settings.Parent.right,
                        FollowerPositionOffsetType.Up => Settings.Parent.up,
                        FollowerPositionOffsetType.Forward => Settings.Parent.forward,
                        _ => Settings.Parent.forward
                    }, Settings.TargetRot)
                    : Settings.TargetRot;
            }

            var lookRotation = Quaternion.LookRotation(targetPosition);
            if (!Settings.SmoothFollow.FollowerUseOffsetRotationAsPosition && !Settings.SmoothFollow.FollowerOffsetPositionIsRelative)
            {
                lookRotation *= Quaternion.Inverse(Quaternion.Euler(Settings.TargetRot));
            }

            _transformer.Position = Vector3.zero;

            if (_wasInMovementScript)
            {
                Cam.Transformer.Position = Settings.TargetPos;
                _wasInMovementScript = false;
            }

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
            Settings.ParentReset();
        }
    }
}