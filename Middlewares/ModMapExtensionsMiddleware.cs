using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using UnityEngine;

namespace Camera2.Middlewares
{
    internal class ModMapExtensionsMiddleware : CamMiddleware, IMHandler
    {
        private Transform _noodleOrigin;

        public bool Pre()
        {
            // We want to parent FP cams as well so that the noodle translations are applied instantly and don't get smoothed out by SmoothFollow
            if (
                !enabled
                || !HookLeveldata.IsModdedMap
                || (!Settings.ModMapExtensions.MoveWithMap && Settings.IsPositionalCam())
            )
            {
                RemoveTransformer(TransformerTypeAndOrder.ModMapParenting);
                _noodleOrigin = null;
                return true;
            }

            _noodleOrigin ??= (GameObject.Find("NoodlePlayerTrackHead") ?? GameObject.Find("NoodlePlayerTrackRoot"))?.transform;

            // Noodle maps do not *necessarily* have a player track if it not actually used
            if (_noodleOrigin != null)
            {
                // If we are not yet attached, and we don't have a parent that's active yet, try to get one!
                AddTransformer(TransformerTypeAndOrder.ModMapParenting);

                Transformer.Position = _noodleOrigin.localPosition;
                Transformer.Rotation = _noodleOrigin.localRotation;

                return true;
            }

            if (_noodleOrigin is null)
            {
                return true;
            }

            Transformer.Position = Vector3.zero;
            Transformer.Rotation = Quaternion.identity;

            _noodleOrigin = null;

            return true;
        }

        public void Post() { }

        public void CamConfigReloaded() { }
    }
}