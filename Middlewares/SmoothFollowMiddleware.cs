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
        private Transform _parent;
        private bool _useLocalPosition;

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
            // only handle first person and attached types
            if (Settings.IsPositionalCam())
            {
                RemoveTransformer(TransformerTypeAndOrder.SmoothFollow);
                _parent = null;
                return true;
            }

            if (Transformer == null)
            {
                AddTransformer(TransformerTypeAndOrder.SmoothFollow);
                TeleportOnNextFrame = true;
            }

            Vector3 targetPosition;
            Quaternion targetRotation;

            if (Settings.Type == CameraType.Attached)
            {
                if (Settings.Parent == null)
                {
                    return false;
                }

                targetPosition = Settings.Parent.position;
                targetRotation = Settings.Parent.rotation;
            }
            else
            {
                Transform parentToUse = null;
                var currentReplaySource = GetCurrentReplaySourceIfAny();

                if (Settings.Type == CameraType.FirstPerson && HookFPFCToggle.isInFPFC)
                {
                    parentToUse = HookFPFCToggle.fpfcTransform;
                    currentReplaySource = null;
                    _useLocalPosition = HookFPFCToggle.isSiraSettingLocalPostionYes;
                }

                if (currentReplaySource == null)
                {
                    if (parentToUse == null)
                    {
                        parentToUse = _parent;
                    }

                    if (parentToUse == null || !parentToUse.gameObject.activeInHierarchy)
                    {
                        var mainCamera = Camera.main;
                        _parent = parentToUse = mainCamera == null ? null : mainCamera.transform;
                        _useLocalPosition = !ScoreSaber.IsInReplayProp;
                    }

                    // If we don't have a parent we should not render.
                    if (parentToUse == null)
                    {
                        return false;
                    }

                    if (_useLocalPosition)
                    {
                        targetPosition = parentToUse.localPosition;
                        targetRotation = parentToUse.localRotation;

                        if (Settings.Type == CameraType.FirstPerson && (HookRoomAdjust.Position != Vector3.zero || HookRoomAdjust.Rotation != Quaternion.identity))
                        {
                            if (!HookFPFCToggle.isInFPFC)
                            {
                                targetPosition = (HookRoomAdjust.Rotation * targetPosition) + HookRoomAdjust.Position;
                                targetRotation = HookRoomAdjust.Rotation * targetRotation;
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

                if (!TeleportOnNextFrame)
                {
                    TeleportOnNextFrame = _lastScene != SceneUtil.CurrentScene || (HookFPFCToggle.isInFPFC && currentReplaySource == null);
                }
            }

            if (!HookFPFCToggle.isInFPFC)
            {
                CalculateLimits(ref targetPosition, ref targetRotation);
            }

            // If we switched scenes (E.g. left / entered a song) we want to snap to the correct position before smoothing again
            if (TeleportOnNextFrame)
            {
                Transformer!.Position = targetPosition;
                Transformer.Rotation = targetRotation;

                _lastScene = SceneUtil.CurrentScene;
                TeleportOnNextFrame = false;
            }
            else
            {
                Transformer!.Position = Vector3.Lerp(Transformer.Position, targetPosition, Cam.TimeSinceLastRender * Settings.SmoothFollow.Position);
                Transformer.Rotation = Quaternion.Slerp(Transformer.Rotation, targetRotation, Cam.TimeSinceLastRender * Settings.SmoothFollow.Rotation);
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