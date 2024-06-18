using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using System;
using UnityEngine;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Middlewares
{
    internal class Follow360Middleware : CamMiddleware, IMHandler
    {
        public void OnDisable() => Reset();
        private float _currentRotateAmount;

        private void Reset()
        {
            if (Transformer == null)
            {
                return;
            }

            _currentRotateAmount = 0f;
            RemoveTransformer(TransformerTypeAndOrder.Follow360);
        }

        public bool Pre()
        {
            if (
                !enabled
                || !Settings.Follow360.Enabled
                || SceneUtil.IsInMenu 
                || !HookLeveldata.Is360Level 
                || HookLevelRotation.Instance == null 
                || !Settings.IsPositionalCam()
            )
            {
                Reset();

                return true;
            }

            if (Transformer == null)
            {
                AddTransformer(TransformerTypeAndOrder.Follow360, true);
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
            if (Math.Abs(_currentRotateAmount - HookLevelRotation.Instance.targetRotation) > .3f)
            {
                rotateStep = Mathf.LerpAngle(_currentRotateAmount, HookLevelRotation.Instance.targetRotation, Cam.TimeSinceLastRender * Settings.Follow360.Smoothing);
            }
            
            var rot = Quaternion.Euler(0, rotateStep, 0);

            Transformer!.Position = (rot * (Cam.Transformer.Position - HookRoomAdjust.Position)) + HookRoomAdjust.Position - Cam.Transformer.Position;

            Transformer.Rotation = Settings.Type == CameraType.Positionable 
                ? rot 
                : Quaternion.identity;
            
            _currentRotateAmount = rotateStep;

            return true;
        }

        public void Post() { }

        public void CamConfigReloaded() { }
    }
}