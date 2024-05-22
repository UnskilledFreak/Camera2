using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using System;
using UnityEngine;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Middlewares
{
    internal class Follow360 : CamMiddleware, IMHandler
    {
        public void OnDisable() => Reset();

        private Transformer _rotationApplier;
        private float _currentRotateAmount;

        private void Reset()
        {
            if (_rotationApplier == null)
            {
                return;
            }

            _currentRotateAmount = 0f;
            _rotationApplier.Rotation = Quaternion.identity;
            _rotationApplier.Position = Vector3.zero;
        }

        public new bool Pre()
        {
            if (
                !enabled
                || !Settings.Follow360.Enabled
                || SceneUtil.IsInMenu 
                || !HookLeveldata.Is360Level 
                || HookLevelRotation.Instance == null 
                || Settings.Type != CameraType.Positionable
            )
            {
                Reset();

                return true;
            }

            if (_rotationApplier == null)
            {
                _rotationApplier = Cam.TransformChain.AddOrGet("Follow360", TransformerOrders.Follow360);
                _rotationApplier.ApplyAsAbsolute = true;
            }

            if (HookLevelRotation.Instance.targetRotation == 0f)
            {
                return true;
            }

            if (_currentRotateAmount == HookLevelRotation.Instance.targetRotation)
            {
                return true;
            }

            var rotateStep = HookLevelRotation.Instance.targetRotation;

            // Make sure we don't spam unnecessary calculations / rotation steps for the last little bit
            if (Math.Abs(_currentRotateAmount - HookLevelRotation.Instance.targetRotation) > 0.3f)
            {
                rotateStep = Mathf.LerpAngle(_currentRotateAmount, HookLevelRotation.Instance.targetRotation, Cam.TimeSinceLastRender * Settings.Follow360.Smoothing);
            }

            var rot = Quaternion.Euler(0, rotateStep, 0);

            _rotationApplier.Position = (rot * (Cam.Transformer.Position - HookRoomAdjust.position)) + HookRoomAdjust.position;
            _rotationApplier.Position -= Cam.Transformer.Position;

            _rotationApplier.Rotation = Settings.Type == CameraType.Positionable 
                ? rot 
                : Quaternion.identity;

            _currentRotateAmount = rotateStep;

            return true;
        }
    }
}