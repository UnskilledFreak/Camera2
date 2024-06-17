﻿using Camera2.Configuration;
using Camera2.Interfaces;
using Camera2.Managers;
using Camera2.Utils;
using System.Linq;
using Camera2.Enums;
using UnityEngine;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Middlewares
{
    internal class MovementScriptProcessorMiddleware : CamMiddleware, IMHandler
    {
        private static readonly System.Random RandomSource = new System.Random();

        private Transformer _scriptTransformer;
        private MovementScript _loadedScript;
        private float _currentAnimationTime;
        private int _frameIndex;
        private float _lastFov;
        private Vector3 _lastPos = Vector3.zero;
        private Quaternion _lastRot = Quaternion.identity;
        private float _lastAnimTime;
        private MovementScript.Frame TargetFrame => _loadedScript.Frames[_frameIndex];

        private void Reset()
        {
            if (_scriptTransformer != null)
            {
                _scriptTransformer.Position = Vector3.zero;
                _scriptTransformer.Rotation = Quaternion.identity;

                if (Settings.MovementScript.FromOrigin)
                {
                    Cam.Settings.ApplyPositionAndRotation();
                }
            }

            _currentAnimationTime = 0f;
            _lastAnimTime = 0;

            if (_loadedScript == null)
            {
                return;
            }

            _loadedScript = null;
            _frameIndex = 0;
            _lastFov = 0f;
            Cam.Camera.fieldOfView = Settings.FOV;
        }

        public void Post() { }

        public void CamConfigReloaded() => Reset();

        public void OnDisable() => Reset();

        public bool Pre()
        {
            if (
                Settings.MovementScript.ScriptList.Length == 0
                || (!SceneUtil.IsInSong && !Settings.MovementScript.EnableInMenu)
                || Cam.Settings.Type != CameraType.Positionable
            )
            {
                Reset();
                return true;
            }

            if (_loadedScript == null)
            {
                var possibleScripts = Settings.MovementScript.ScriptList.Where(MovementScriptManager.MovementScripts.ContainsKey).ToArray();

                if (possibleScripts.Length == 0)
                {
                    return true;
                }

                var scriptToUse = possibleScripts[RandomSource.Next(possibleScripts.Length)];

                _loadedScript = MovementScriptManager.MovementScripts[scriptToUse];

                if (_loadedScript == null)
                {
                    return true;
                }

                _lastFov = Cam.Camera.fieldOfView;

                Cam.LogInfo($"Applying Movement script {scriptToUse} for camera {Cam.Name}");

                _scriptTransformer ??= Cam.TransformChain.AddOrGet(TransformerTypeAndOrder.MovementScriptProcessor);
            }

            if (_loadedScript.SyncToSong && SceneUtil.IsInSong)
            {
                /*
                 * In MP the TimeSyncController doesn't exist in the countdown phase,
                 * so if the script is synced to the song we'll just hard lock the time
                 * at 0 if the controller doesn't exist
                 */
                _currentAnimationTime = !SceneUtil.HasSongPlayer ? 0 : SceneUtil.AudioTimeSyncController.songTime;
            }
            else
            {
                _currentAnimationTime += Cam.TimeSinceLastRender;
            }

            if (Settings.MovementScript.FromOrigin)
            {
                Cam.Transformer.Position = Vector3.zero;
                Cam.Transformer.Rotation = Quaternion.identity;
            }

            if (_currentAnimationTime > _loadedScript.ScriptDuration)
            {
                if (!_loadedScript.Loop)
                {
                    /*
                    if (_scriptTransformer != null)
                    {
                        Cam.TransformChain.Remove(TransformerOrders.MovementScriptProcessor);
                        _scriptTransformer = null;
                        Cam.Transformer.Position = _lastPos;
                        Cam.Transformer.Rotation = _lastRot;
                    }
                    */
                    return true;
                }

                _currentAnimationTime %= _loadedScript.ScriptDuration;
                _lastAnimTime = _currentAnimationTime;
                _frameIndex = 0;
            }

            for (;;)
            {
                // Rollback logic for skipping through replays
                if (_lastAnimTime > _currentAnimationTime)
                {
                    while (_frameIndex > 0)
                    {
                        _frameIndex--;

                        if (TargetFrame.StartTime <= _currentAnimationTime)
                        {
                            break;
                        }
                    }

                    _lastAnimTime = _currentAnimationTime;
                }

                if (TargetFrame.StartTime > _currentAnimationTime)
                {
                    break;
                }

                if (TargetFrame.TransitionEndTime <= _currentAnimationTime)
                {
                    _lastPos = _scriptTransformer.Position = TargetFrame.Position;
                    _lastRot = _scriptTransformer.Rotation = TargetFrame.Rotation;
                    if (TargetFrame.FOV > 0)
                    {
                        _lastFov = Cam.Camera.fieldOfView = TargetFrame.FOV;
                    }
                }
                else if (TargetFrame.StartTime <= _currentAnimationTime)
                {
                    var frameProgress = (_currentAnimationTime - TargetFrame.StartTime) / TargetFrame.Duration;

                    if (TargetFrame.Transition == MoveType.Eased)
                    {
                        frameProgress = Easings.EaseInOutCubic01(frameProgress);
                    }

                    _scriptTransformer.Position = Vector3.LerpUnclamped(_lastPos, TargetFrame.Position, frameProgress);
                    _scriptTransformer.Rotation = Quaternion.LerpUnclamped(_lastRot, TargetFrame.Rotation, frameProgress);

                    if (TargetFrame.FOV > 0f)
                    {
                        Cam.Camera.fieldOfView = Mathf.LerpUnclamped(_lastFov, TargetFrame.FOV, frameProgress);
                    }

                    break;
                }

                if (++_frameIndex < _loadedScript.Frames.Count)
                {
                    continue;
                }

                _frameIndex = 0;
                break;
            }

            _lastAnimTime = _currentAnimationTime;

            return true;
        }
    }
}