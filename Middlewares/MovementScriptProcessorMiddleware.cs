using System.Linq;
using Camera2.Interfaces;
using Camera2.Managers;
using Camera2.Utils;
using Camera2.Enums;
using Camera2.MovementScript;
using UnityEngine;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Middlewares
{
    internal class MovementScriptProcessorMiddleware : CamMiddleware, IMHandler
    {
        private Script _loadedScript;
        private float _currentAnimationTime;
        private int _frameIndex;
        private float _lastFov;
        private Vector3 _lastPos = Vector3.zero;
        private Quaternion _lastRot = Quaternion.identity;
        private float _lastAnimTime;
        private ScriptFrame TargetScriptFrame => _loadedScript.Frames[_frameIndex];

        private void Reset()
        {
            if (Transformer != null)
            {
                Transformer.Position = Vector3.zero;
                Transformer.Rotation = Quaternion.identity;

                if (Chain.HasType(TransformerTypeAndOrder.MovementScriptProcessor))
                {
                    RemoveTransformer(TransformerTypeAndOrder.MovementScriptProcessor);
                }
                
                /*
                if (Settings.MovementScript.FromOrigin)
                {
                    Cam.Settings.ApplyPositionAndRotation();
                }
                */
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
                Cam.Settings.Type == CameraType.FirstPerson
                || Cam.Settings.Type == CameraType.Attached
                || Settings.MovementScript.ScriptList.Length == 0
                || (!SceneUtil.IsInSong && !Settings.MovementScript.EnableInMenu)
            )
            {
                Reset();
                return true;
            }

            if (_loadedScript == null)
            {
                _loadedScript = MovementScriptManager.GetRandomFromPossibles(Settings.MovementScript.ScriptList);

                if (_loadedScript == null)
                {
                    return true;
                }

                //_lastFov = Cam.Camera.fieldOfView;
                _lastFov = Settings.FOV;
                Cam.Camera.fieldOfView = Settings.FOV;

                Cam.LogInfo($"Applying Movement script {_loadedScript.Name} for camera {Cam.Name}");

                AddTransformer(TransformerTypeAndOrder.MovementScriptProcessor);
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
                    if (Settings.Type == CameraType.Follower)
                    {
                        Chain.Remove(TransformerTypeAndOrder.MovementScriptProcessor);
                        Settings.UnOverriden(delegate
                        {
                            Settings.TargetPosition = _loadedScript.Frames.Last().Position;
                        });
                        Settings.ApplyPositionAndRotation();
                    }

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

                        if (TargetScriptFrame.StartTime <= _currentAnimationTime)
                        {
                            break;
                        }
                    }

                    _lastAnimTime = _currentAnimationTime;
                }

                if (TargetScriptFrame.StartTime > _currentAnimationTime)
                {
                    break;
                }

                if (TargetScriptFrame.TransitionEndTime <= _currentAnimationTime)
                {
                    _lastPos = Transformer.Position = TargetScriptFrame.Position;
                    if (!TargetScriptFrame.OnFollowerIgnoreRotation)
                    {
                        _lastRot = Transformer.Rotation = TargetScriptFrame.Rotation;
                    }
                    if (TargetScriptFrame.FOV > 0)
                    {
                        _lastFov = Cam.Camera.fieldOfView = TargetScriptFrame.FOV;
                    }
                }
                else if (TargetScriptFrame.StartTime <= _currentAnimationTime)
                {
                    var frameProgress = (_currentAnimationTime - TargetScriptFrame.StartTime) / TargetScriptFrame.Duration;

                    if (TargetScriptFrame.Transition == MoveType.Eased)
                    {
                        frameProgress = Easings.EaseInOutCubic01(frameProgress);
                    }

                    Transformer.Position = Vector3.LerpUnclamped(_lastPos, TargetScriptFrame.Position, frameProgress);
                    if (!TargetScriptFrame.OnFollowerIgnoreRotation)
                    {
                        Transformer.Rotation = Quaternion.LerpUnclamped(_lastRot, TargetScriptFrame.Rotation, frameProgress);
                    }

                    if (TargetScriptFrame.FOV > 0f)
                    {
                        Cam.Camera.fieldOfView = Mathf.LerpUnclamped(_lastFov, TargetScriptFrame.FOV, frameProgress);
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
        
        public void ForceReset() { }
    }
}