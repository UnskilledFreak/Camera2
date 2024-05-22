using System;
using System.Linq;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.SDK;
using Camera2.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Middlewares
{
    internal class SmoothFollow : CamMiddleware, IMHandler
    {
        private Scene _lastScene;
        private bool _teleportOnNextFrame;

        private Transform Parent
        {
            get => Settings.SmoothFollow.Parent;
            set => Settings.SmoothFollow.Parent = value;
        }

        public void OnEnable()
        {
            /*
             * If the camera was just enabled we want to teleport the positon / rotation.
             * This is useful when you switch to a scene with a firstperson camera that was
             * not enabled for a while to make it have a "correct" initial position instead
             * of smoothing it to the correct position over time
             */
            _teleportOnNextFrame = true;
        }

        private static float ClampAngle(float angle, float from, float to)
        {
            // accepts e.g. -80, 80
            if (angle < 0f)
            {
                angle = 360 + angle;
            }

            return angle > 180f
                ? Math.Max(angle, 360 + from)
                : Math.Min(angle, to);
        }

        public new bool Pre()
        {
            if (Settings.Type == CameraType.Positionable)
            {
                if (Settings.SmoothFollow.Transformer == null)
                {
                    return true;
                }

                Settings.SmoothFollow.Transformer.Position = Vector3.zero;
                Settings.SmoothFollow.Transformer.Rotation = Quaternion.identity;

                return true;
            }
            
            /*
            if (Settings.Type == CameraType.Follower)
            {
                if (Settings.SmoothFollow.Transformer != null)
                {
                    var target = GameObject.Find(Cam.Settings.SmoothFollow.TargetParent);
                    if (target == null)
                    {
                        return true;
                    }

                    Settings.SmoothFollow.Transformer.Position = Vector3.zero;
                    Settings.SmoothFollow.Transformer.Rotation = Quaternion.LookRotation(target.transform.position);
                    //Cam.transform.LookAt(target.transform);
                    //Cam.transform.position = Settings.TargetPos;
                    //Cam.transform.Rotate(
                    //    Cam.transform.rotation.x + Settings.TargetRot.x,
                    //    Cam.transform.rotation.y + Settings.TargetRot.y,
                    //    Cam.transform.rotation.z + Settings.TargetRot.z
                    //);
                    return true;
                }
            }
            */

            Transform parentToUse = null;
            ISource currentReplaySource = null;

            if (Settings.Type == CameraType.FirstPerson && Settings.SmoothFollow.FollowReplayPosition)
            {
                foreach (var source in ReplaySources.Sources.Where(source => source.IsInReplay))
                {
                    currentReplaySource = source;
                    break;
                }
            }

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
                            var a = Camera.main;
                            Parent = parentToUse = a == null ? null : a.transform;
                            // this is because ScoreSaber uses absolute positioning, thanks for explaining...
                            Settings.SmoothFollow.UseLocalPosition = !ScoreSaber.IsInReplayProp;
                            break;
                        case CameraType.Attached:
                        //case CameraType.Follower:
                            Parent = parentToUse = GameObject.Find(Cam.Settings.SmoothFollow.TargetParent)?.transform;
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
                        else
                        {
                            //var parentsParent = parentToUse.parent;
                            //var parentsParentLocalRotation = parentsParent.rotation;
                            //targetPosition += parentsParent.localPosition;
                            //targetRotation *= Quaternion.Inverse(parentsParentLocalRotation);
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
                // TODO: This is kinda shit
                var l = Settings.SmoothFollow.Limits;

                if (!float.IsNegativeInfinity(l.PosXMin) || !float.IsPositiveInfinity(l.PosXMax))
                {
                    targetPosition.x = Mathf.Clamp(targetPosition.x, l.PosXMin, l.PosXMax);
                }

                if (!float.IsNegativeInfinity(l.PosYMin) || !float.IsPositiveInfinity(l.PosYMax))
                {
                    targetPosition.y = Mathf.Clamp(targetPosition.y, l.PosYMin, l.PosYMax);
                }

                if (!float.IsNegativeInfinity(l.PosZMin) || !float.IsPositiveInfinity(l.PosZMax))
                {
                    targetPosition.z = Mathf.Clamp(targetPosition.z, l.PosZMin, l.PosZMax);
                }

                var eulerAngles = targetRotation.eulerAngles;

                if (!float.IsNegativeInfinity(l.RotXMin) || !float.IsPositiveInfinity(l.RotXMax))
                {
                    eulerAngles.x = ClampAngle(eulerAngles.x, l.RotXMin, l.RotXMax);
                }

                if (!float.IsNegativeInfinity(l.RotYMin) || !float.IsPositiveInfinity(l.RotYMax))
                {
                    eulerAngles.y = ClampAngle(eulerAngles.y, l.RotYMin, l.RotYMax);
                }

                if (!float.IsNegativeInfinity(l.RotZMin) || !float.IsPositiveInfinity(l.RotZMax))
                {
                    eulerAngles.z = ClampAngle(eulerAngles.z, l.RotZMin, l.RotZMax);
                }

                targetRotation.eulerAngles = eulerAngles;
            }

            if (!_teleportOnNextFrame)
            {
                _teleportOnNextFrame = _lastScene != SceneUtil.CurrentScene
                                       || (HookFPFCToggle.isInFPFC && currentReplaySource == null);
            }

            if (Settings.SmoothFollow.Transformer == null)
            {
                Settings.SmoothFollow.Transformer = Cam.TransformChain.AddOrGet("SmoothFollow", TransformerOrders.SmoothFollow);

                _teleportOnNextFrame = true;
            }

            var theTransform = Settings.SmoothFollow.Transformer;

            // If we switched scenes (E.g. left / entered a song) we want to snap to the correct position before smoothing again
            if (_teleportOnNextFrame)
            {
                theTransform.Position = targetPosition;
                theTransform.Rotation = targetRotation;

                _lastScene = SceneUtil.CurrentScene;
                _teleportOnNextFrame = false;
            }
            else
            {
                theTransform.Position = Vector3.Lerp(theTransform.Position, targetPosition, Cam.TimeSinceLastRender * Settings.SmoothFollow.Position);
                theTransform.Rotation = Quaternion.Slerp(theTransform.Rotation, targetRotation, Cam.TimeSinceLastRender * Settings.SmoothFollow.Rotation);
            }

            return true;
        }
    }
}