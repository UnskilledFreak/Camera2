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
            var realTargetPosition = -(Cam.Camera.transform.localPosition - Settings.Parent.position);
            // where is clone?
            var targetPositionWitOffset = new Vector3(realTargetPosition.x, realTargetPosition.y, realTargetPosition.z);
            if (Settings.SmoothFollow.FollowerUseOffsetRotationAsPosition)
            {
                targetPositionWitOffset += Settings.SmoothFollow.FollowerOffsetPositionIsRelative
                    ? Vector3.Scale(Settings.SmoothFollow.FollowerOffsetPositionRelativeType switch
                    {
                        FollowerPositionOffsetType.Right => Settings.Parent.right,
                        FollowerPositionOffsetType.Left => -Settings.Parent.right,
                        FollowerPositionOffsetType.Up => Settings.Parent.up,
                        FollowerPositionOffsetType.Down => -Settings.Parent.up,
                        FollowerPositionOffsetType.Forward => Settings.Parent.forward,
                        FollowerPositionOffsetType.Backward => -Settings.Parent.forward,
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

            var lookRotation = Quaternion.LookRotation(targetPositionWitOffset, upVector);

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
                TeleportOnNextFrame = false;
                /*
                Transformer.Rotation = _wasInMovementScript
                    ? lookRotation
                    : Quaternion.identity;
                */
                Transformer.Rotation = lookRotation;
            }
            else
            {
                Transformer.Rotation = Quaternion.Slerp(Transformer.Rotation, lookRotation, Cam.TimeSinceLastRender * Settings.SmoothFollow.Rotation);
            }

            if (Settings.SmoothFollow.FollowerFakeZoom.IsValid())
            {
                //var distance = Mathf.Abs(Vector3.Distance(Cam.Camera.transform.localPosition, Settings.Parent.position + positionOffset));
                var distance = Mathf.Abs(Vector3.Distance(Cam.Camera.transform.localPosition, Settings.SmoothFollow.FollowerFakeZoom.IgnorePositionOffset ? realTargetPosition : targetPositionWitOffset));
                var fovDelta = Settings.SmoothFollow.FollowerFakeZoom.FarthestFOV - Settings.SmoothFollow.FollowerFakeZoom.NearestFOV;
                //var fov = Mathf.Clamp(Settings.SmoothFollow.FollowerFakeZoom.MaxFOV - (Settings.SmoothFollow.FollowerFakeZoom.Distance * Mathf.Log(distance)), Settings.SmoothFollow.FollowerFakeZoom.MinFOV, Settings.SmoothFollow.FollowerFakeZoom.MaxFOV);
                var fov = Mathf.Clamp(distance / Settings.SmoothFollow.FollowerFakeZoom.Distance, .01f, 1f) * fovDelta + Settings.SmoothFollow.FollowerFakeZoom.NearestFOV;
                //Cam.LogInfo("distance: " + distance + " - FOV = " + fov);
                Cam.Camera.fieldOfView = fov;
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