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
            if (Settings.Type != CameraType.Follower)
            {
                RemoveTransformer(TransformerTypeAndOrder.Follower);
                return true;
            }
            
            if (Settings.Parent == null)
            {
                // do render like a positionable would if no target is set
                return true;
            }

            // don't track until MovementScript is done
            if (Chain.HasType(TransformerTypeAndOrder.MovementScriptProcessor))
            {
                // remove follower chain so rotations won't stick
                RemoveTransformer(TransformerTypeAndOrder.Follower);
                _wasInMovementScript = true;
                return true;
            }

            if (Transformer == null)
            {
                TeleportOnNextFrame = true;

                AddTransformer(TransformerTypeAndOrder.Follower);
            }

            // position to look at, not the cams position
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
                    }, Settings.TargetRotation)
                    : Settings.TargetRotation;
            }

            // calculate the vector where "up" is, this is only used for noodle maps with player movement
            var upVector = Vector3.up;
            if (Chain.HasType(TransformerTypeAndOrder.ModMapParenting))
            {
                var transformer = GetTransformer(TransformerTypeAndOrder.ModMapParenting);
                if (transformer.Rotation.eulerAngles.z != 0)
                {
                    upVector = (Cam.Transformer.Position - GetTransformer(TransformerTypeAndOrder.ModMapParenting).Position).normalized;
                }
            }

            var lookRotation = Quaternion.LookRotation(targetPosition, upVector);

            // calculate the rotation offset
            if (!Settings.SmoothFollow.FollowerUseOffsetRotationAsPosition && !Settings.SmoothFollow.FollowerOffsetPositionIsRelative)
            {
                var rotOffset = Quaternion.Euler(Settings.TargetRotation);
                if (Settings.SmoothFollow.FollowerUseOrganic)
                {
                    rotOffset = Quaternion.Inverse(rotOffset);
                }
                lookRotation *= rotOffset;
            }


            if (_wasInMovementScript)
            {
                _wasInMovementScript = false;
            }
            
            Transformer!.Position = Vector3.zero;

            if (TeleportOnNextFrame)
            {
                Transformer!.Rotation = Quaternion.identity;
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
        
        public void ForceReset() { }
    }
}