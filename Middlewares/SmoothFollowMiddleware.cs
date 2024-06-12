using System.Linq;
using Camera2.Extensions;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.SDK;
using Camera2.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Middlewares
{
    internal class SmoothFollowMiddleware : CamMiddleware, IMHandler
    {
        private Scene _lastScene;

        private Transform Parent
        {
            get => Settings.SmoothFollow.Parent;
            set => Settings.SmoothFollow.Parent = value;
        }

        public void OnEnable()
        {
            /*
             * If the camera was just enabled we want to teleport the position / rotation.
             * This is useful when you switch to a scene with a first person camera that was
             * not enabled for a while to make it have a "correct" initial position instead
             * of smoothing it to the correct position over time
             */
            TeleportOnNextFrame = true;
        }

        public bool Pre()
        {
            if (Settings.IsPositionalCam())
            {
                // this fixes newly added cams to behave weird when grabbed
                // also fixes position and rotation offset being applied multiple times when they should not
                // reason why it is here? when creating a new cam, it will be a FirstPerson type one and
                // SmoothFollow will add its Transformer to it, maybe it should remove the Transformer?
                if (Settings.SmoothFollow.Transformer != null)
                {
                    Settings.SmoothFollow.Transformer.Position = Vector3.zero;
                    Settings.SmoothFollow.Transformer.Rotation = Quaternion.identity;
                }
                return true;
            }

            Transform parentToUse = null;
            var currentReplaySource = GetCurrentReplaySourceIfAny();

            if (Settings.Type == CameraType.FirstPerson && HookFPFCToggle.isInFPFC)
            {
                parentToUse = HookFPFCToggle.fpfcTransform;
                currentReplaySource = null;
                Settings.SmoothFollow.UseLocalPosition = HookFPFCToggle.isSiraSettingLocalPostionYes;
            }

            Vector3 targetPosition;
            Quaternion targetRotation;

            if (currentReplaySource == null)
            {
                if (parentToUse == null)
                {
                    parentToUse = Parent;
                }

                if (parentToUse == null || !parentToUse.gameObject.activeInHierarchy)
                {
                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (Settings.Type)
                    {
                        case CameraType.FirstPerson:
                            var mainCamera = Camera.main;
                            Parent = parentToUse = mainCamera == null ? null : mainCamera.transform;
                            Settings.SmoothFollow.UseLocalPosition = !ScoreSaber.IsInReplayProp;
                            break;
                        case CameraType.Attached:
                            Parent = parentToUse = Settings.Parent;
                            Settings.SmoothFollow.UseLocalPosition = false;
                            break;
                    }
                }

                // If we don't have a parent we should not render.
                if (parentToUse == null)
                {
                    return false;
                }

                if (Settings.SmoothFollow.UseLocalPosition)
                {
                    targetPosition = parentToUse.localPosition;
                    targetRotation = parentToUse.localRotation;

                    if (Settings.Type == CameraType.FirstPerson && (HookRoomAdjust.position != Vector3.zero || HookRoomAdjust.rotation != Quaternion.identity))
                    {
                        if (!HookFPFCToggle.isInFPFC)
                        {
                            targetPosition = (HookRoomAdjust.rotation * targetPosition) + HookRoomAdjust.position;
                            targetRotation = HookRoomAdjust.rotation * targetRotation;
                        }
                    }
                }
                else
                {
                    targetPosition = parentToUse.position;
                    targetRotation = parentToUse.rotation;
                }
            }
            else
            {
                targetPosition = currentReplaySource.localHeadPosition;
                targetRotation = currentReplaySource.localHeadRotation;
            }

            if (!HookFPFCToggle.isInFPFC)
            {
                CalculateLimits(ref targetPosition, ref targetRotation);
            }

            if (!TeleportOnNextFrame)
            {
                TeleportOnNextFrame = _lastScene != SceneUtil.CurrentScene || (HookFPFCToggle.isInFPFC && currentReplaySource == null);
            }

            if (Settings.SmoothFollow.Transformer == null)
            {
                Settings.SmoothFollow.Transformer = Cam.TransformChain.AddOrGet("SmoothFollow", TransformerOrders.SmoothFollow);
                TeleportOnNextFrame = true;
            }

            var theTransform = Settings.SmoothFollow.Transformer;

            // If we switched scenes (E.g. left / entered a song) we want to snap to the correct position before smoothing again
            if (TeleportOnNextFrame)
            {
                theTransform.Position = targetPosition;
                theTransform.Rotation = targetRotation;

                _lastScene = SceneUtil.CurrentScene;
                TeleportOnNextFrame = false;
            }
            else
            {
                theTransform.Position = Vector3.Lerp(theTransform.Position, targetPosition, Cam.TimeSinceLastRender * Settings.SmoothFollow.Position);
                theTransform.Rotation = Quaternion.Slerp(theTransform.Rotation, targetRotation, Cam.TimeSinceLastRender * Settings.SmoothFollow.Rotation);
            }

            return true;
        }

        public void Post() { }

        public void CamConfigReloaded() { }

        private ISource GetCurrentReplaySourceIfAny()
        {
            if (Settings.Type != CameraType.FirstPerson || !Settings.SmoothFollow.FollowReplayPosition)
            {
                return null;
            }

            return ReplaySources.Sources.FirstOrDefault(source => source.IsInReplay);
        }

        private void CalculateLimits(ref Vector3 targetPosition, ref Quaternion targetRotation)
        {
            targetPosition.InBoundary(
                Settings.SmoothFollow.Limits.PosVectorX,
                Settings.SmoothFollow.Limits.PosVectorY,
                Settings.SmoothFollow.Limits.PosVectorZ
            );

            var eulerAngles = targetRotation.eulerAngles;
            eulerAngles.InBoundary(
                Settings.SmoothFollow.Limits.RotVectorX,
                Settings.SmoothFollow.Limits.RotVectorY,
                Settings.SmoothFollow.Limits.RotVectorZ
            );

            targetRotation.eulerAngles = eulerAngles;
        }
    }
}