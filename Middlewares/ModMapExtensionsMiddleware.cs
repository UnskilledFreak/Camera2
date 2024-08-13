using Camera2.Enums;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Managers;
using Camera2.Utils;
using JetBrains.Annotations;
using UnityEngine;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Middlewares
{
    internal class ModMapExtensionsMiddleware : CamMiddleware, IMHandler
    {
        [CanBeNull]
        private GameObject _noodleOrigin;

        public bool Pre()
        {
            // We want to parent FP cams as well so that the noodle translations are applied instantly and don't get smoothed out by SmoothFollow
            if (
                !enabled
                || !HookLeveldata.IsModdedMap
                || (!Settings.ModMapExtensions.MoveWithMap && Settings.IsPositionalCam())
            )
            {
                ForceReset();
                return true;
            }

            // fix restart of noodle maps results in black screen
            if (_noodleOrigin is null || _noodleOrigin == null)
            {
                _noodleOrigin = null;
            }

            // Noodle maps do not *necessarily* have a player track if it not actually used            
            _noodleOrigin ??= GameObject.Find("NoodlePlayerTrackHead") ?? GameObject.Find("NoodlePlayerTrackRoot");

            if (!(_noodleOrigin is null))
            {
                if (Transformer == null)
                {
                    AddTransformer(TransformerTypeAndOrder.ModMapParenting);
                }
                
                if (!ScenesManager.IsOnCustomScene && !HookFPFCToggle.isInFPFC && ScenesManager.LoadedScene != SceneTypes.PlayingModmap)
                {
                    //Cam.LogInfo("modmap WITH movement");
                    ScenesManager.SwitchToScene(SceneTypes.PlayingModmap);   
                }

                Transformer!.Rotation = _noodleOrigin!.transform.localRotation;
                Transformer!.Position = Settings.Type == CameraType.Follower
                    ? Cam.CalculatePositionOffsetOnRotation(_noodleOrigin.transform.localRotation, _noodleOrigin.transform.localPosition)
                    : _noodleOrigin.transform.localPosition;

                return true;
            }

            if (_noodleOrigin is null)
            {
                /*
                if (!ScenesManager.IsOnCustomScene && !HookFPFCToggle.isInFPFC && ScenesManager.LoadedScene != SceneTypes.PlayingModmapNoMotion)
                {
                    Cam.LogInfo("modmap without movement");
                    //ScenesManager.SwitchToScene(SceneTypes.PlayingModmapNoMotion);
                }
                //*/
                return true;
            }

            Transformer.Position = Vector3.zero;
            Transformer.Rotation = Quaternion.identity;

            _noodleOrigin = null;

            return true;
        }

        public void Post() { }

        public void CamConfigReloaded() { }
        public void ForceReset()
        {
            RemoveTransformer(TransformerTypeAndOrder.ModMapParenting);
            _noodleOrigin = null;
        }
    }
}