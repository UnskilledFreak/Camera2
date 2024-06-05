using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using UnityEngine;

namespace Camera2.Middlewares
{
    internal class ModMapExtensionsMiddleware : CamMiddleware, IMHandler
    {
        //private static Type NoodlePlayerTrack;
        private static Transform globNoodleOrigin;
        private Transform _noodleOrigin;
        private Transformer _mapMovementTransformer;

        public static void Reflect()
        {
            //NoodlePlayerTrack ??= IPA.Loader.PluginManager.GetPluginFromId("NoodleExtensions")?.Assembly.GetType("NoodleExtensions.Animation.PlayerTrack");
        }

        public bool Pre()
        {
            // We want to parent FP cams as well so that the noodle translations are applied instantly and don't get smoothed out by SmoothFollow
            if (
                enabled 
                && HookLeveldata.IsModdedMap
                && (Settings.ModMapExtensions.MoveWithMap || !Settings.IsPositionalCam())
            )
            {
                if (_noodleOrigin is null)
                {
                    // Unity moment
                    if (globNoodleOrigin == null)
                    {
                        globNoodleOrigin = null;
                    }

                    // This stinks
                    _noodleOrigin = globNoodleOrigin ?? (GameObject.Find("NoodlePlayerTrackHead") ?? GameObject.Find("NoodlePlayerTrackRoot"))?.transform;
                    globNoodleOrigin = _noodleOrigin;
                }

                // Noodle maps do not *necessarily* have a player track if it not actually used
                if (_noodleOrigin != null)
                {
                    // If we are not yet attached, and we don't have a parent that's active yet, try to get one!
                    _mapMovementTransformer ??= Cam.TransformChain.AddOrGet("ModMapExt", TransformerOrders.ModMapParenting);

                    _mapMovementTransformer.Position = _noodleOrigin.localPosition;
                    _mapMovementTransformer.Rotation = _noodleOrigin.localRotation;
                    return true;
                }
            }

            if (_noodleOrigin is null)
            {
                return true;
            }

            _mapMovementTransformer.Position = Vector3.zero;
            _mapMovementTransformer.Rotation = Quaternion.identity;

            _noodleOrigin = null;

            return true;
        }

        public void Post() { }

        public void CamConfigReloaded() { }
    }
}