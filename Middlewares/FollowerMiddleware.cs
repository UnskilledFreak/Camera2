using Camera2.Enums;
using Camera2.Interfaces;
using Camera2.Utils;
using UnityEngine;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Middlewares
{
    internal class FollowerMiddleware : CamMiddleware, IMHandler
    {
        private bool _wasInMovementScript;

        public void OnEnable()
        {
            TeleportOnNextFrame = true;
        }

        public bool Pre()
        {
            if (Settings.Type != CameraType.Follower || Settings.Parent == null)
            {
                RemoveTransformer(TransformerTypeAndOrder.Follower);
                return true;
            }

            // don't track until MovementScript is done
            if (Chain.HasType(TransformerTypeAndOrder.MovementScriptProcessor))
            {
                _wasInMovementScript = true;
                return true;
            }

            if (Transformer == null)
            {
                TeleportOnNextFrame = true;

                AddTransformer(TransformerTypeAndOrder.Follower);
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

            var upVector = Chain.HasType(TransformerTypeAndOrder.ModMapParenting)
                ? (Cam.Transformer.Position - GetTransformer(TransformerTypeAndOrder.ModMapParenting).Position).normalized
                : Vector3.up;
            
            var lookRotation = Quaternion.LookRotation(targetPosition, upVector);
            
            if (!Settings.SmoothFollow.FollowerUseOffsetRotationAsPosition && !Settings.SmoothFollow.FollowerOffsetPositionIsRelative)
            {
                lookRotation *= Quaternion.Euler(Settings.TargetRot);
            }

            Transformer!.Position = Vector3.zero;

            if (_wasInMovementScript)
            {
                Cam.Transformer.Position = Settings.TargetPos;
                _wasInMovementScript = false;
            }

            if (TeleportOnNextFrame)
            {
                Transformer.Rotation = Quaternion.identity;
                TeleportOnNextFrame = false;
            }
            else
            {
                Transformer.Rotation = Quaternion.Slerp(Transformer.Rotation, lookRotation, Cam.TimeSinceLastRender * Settings.SmoothFollow.Rotation);
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