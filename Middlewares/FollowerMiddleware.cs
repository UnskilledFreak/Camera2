using System;
using System.Collections.Generic;
using Camera2.Enums;
using Camera2.Extensions;
using Camera2.Interfaces;
using Camera2.Utils;
using JetBrains.Annotations;
using UnityEngine;
using CameraType = Camera2.Enums.CameraType;
using Random = System.Random;

namespace Camera2.Middlewares
{
    internal class FollowerMiddleware : CamMiddleware, IMHandler
    {
        private bool _wasInMovementScript;
        private readonly Dictionary<int, int> _inDrunkCooldown = new Dictionary<int, int>();
        private Vector3 _drunkPosition = Vector3.zero;
        private Vector3 _drunkRotation = Vector3.zero;
        private Vector3 _drunkPositionOffset = Vector3.zero;

        public void OnEnable()
        {
            TeleportOnNextFrame = true;
        }

        public bool Pre()
        {
            if (!Settings.IsFollowerCam())
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
                Settings.ApplyPositionAndRotation();
            }

            // position to look at, not the cams position, why do I have to negate it? ö.o
            var realTargetPosition = -(Cam.Camera.transform.localPosition - Settings.Parent.position);
            
            GetRandomIf(1, 80, 20, 20, 20, -10, 10, ref _drunkPositionOffset, null);
            var targetPositionWitOffset = realTargetPosition.Clone() + _drunkPositionOffset;
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


            if (TeleportOnNextFrame)
            {
                TeleportOnNextFrame = false;
                /*
                Transformer.Rotation = _wasInMovementScript
                    ? lookRotation
                    : Quaternion.identity;
                */
                Transformer!.Rotation = lookRotation;
            }
            else
            {
                GetRandomIf(2, 70, 30, 30, 45, -30, 30, ref _drunkRotation, null);
                Transformer!.Rotation = Transformer.Rotation.Slerp(lookRotation * Quaternion.Euler(_drunkRotation), GetSlerpTime( Settings.SmoothFollow.Rotation, GetRandomNumber(5f, 50f)));
            }

            GetRandomIf(3, 50, 20, 5, 10, -1.5f, 1.5f, ref _drunkPosition, v => Transformer.Position + v);
            if (Settings.Type == CameraType.FollowerDrunk)
            {
                Cam.LogInfo($"Pos: {Transformer.Position} / Target: {_drunkPosition}");
            }

            if (Settings.Type == CameraType.FollowerDrunk)
            {
                if (_drunkPosition != Vector3.zero)
                {
                    Transformer.Position = Transformer.Position.Slerp(_drunkPosition, GetSlerpTime(Settings.SmoothFollow.Position, GetRandomNumber(50f, 150f)));
                }
            }
            else
            {
                Transformer.Position = Vector3.zero;
            }

            if (Settings.SmoothFollow.FollowerFakeZoom.IsValid())
            {
                //var distance = Mathf.Abs(Vector3.Distance(Cam.Camera.transform.localPosition, Settings.Parent.position + positionOffset));
                var distance = Mathf.Abs(Vector3.Distance(Cam.Camera.transform.localPosition, Settings.SmoothFollow.FollowerFakeZoom.IgnorePositionOffset ? realTargetPosition : targetPositionWitOffset));
                var fovDelta = Settings.SmoothFollow.FollowerFakeZoom.FarthestFOV - Settings.SmoothFollow.FollowerFakeZoom.NearestFOV;
                //var fov = Mathf.Clamp(Settings.SmoothFollow.FollowerFakeZoom.MaxFOV - (Settings.SmoothFollow.FollowerFakeZoom.Distance * Mathf.Log(distance)), Settings.SmoothFollow.FollowerFakeZoom.MinFOV, Settings.SmoothFollow.FollowerFakeZoom.MaxFOV);
                var fov = (Mathf.Clamp(distance / Settings.SmoothFollow.FollowerFakeZoom.Distance, .01f, 1f) * fovDelta) + Settings.SmoothFollow.FollowerFakeZoom.NearestFOV;
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

        public void ForceReset()
        {
            _inDrunkCooldown.Clear();
            _drunkPosition = Vector3.zero;
            _drunkPositionOffset  = Vector3.zero;
            _drunkRotation  = Vector3.zero;
        }

        private float GetSlerpTime(float multiplier, float drunk = 1f) => Cam.TimeSinceLastRender * (Settings.Type == CameraType.FollowerDrunk ? multiplier / drunk : multiplier);

        private void GetRandomIf(int t, int percentage, int p1, int p2, int p3, float min, float max, ref Vector3 writeTo, [CanBeNull] Func<Vector3, Vector3> callback)
        {
            if (!_inDrunkCooldown.ContainsKey(t))
            {
                _inDrunkCooldown.Add(t, 0);
            }

            if (Settings.Type != CameraType.FollowerDrunk || !IsRandomHeck(percentage))
            {
                return;
            }

            if (_inDrunkCooldown[t] == 0)
            {
                _inDrunkCooldown[t] = 1500;
                writeTo = GetRandomVector(p1, p2, p3, min, max);
                if (callback != null)
                {
                    writeTo = callback(writeTo);
                }
            }

            _inDrunkCooldown[t]--;
        }

        private static bool IsRandomHeck(int percentage) => new Random().Next(0, 100) < percentage;

        private static Vector3 GetRandomVector(int p1, int p2, int p3, float min, float max)
        {
            return new Vector3(
                IsRandomHeck(p1) ? GetRandomNumber(min, max) : 0,
                IsRandomHeck(p2) ? GetRandomNumber(min, max) : 0,
                IsRandomHeck(p3) ? GetRandomNumber(min, max) : 0
            );
        }
        
        private static float GetRandomNumber(float minimum, float maximum)
        { 
            var random = new Random();
            return (float)((random.NextDouble() * (maximum - minimum)) + minimum);
        }
    }
}