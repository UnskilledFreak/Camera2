using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using UnityEngine;

namespace Camera2.Middlewares
{
    internal class MultiplayerMiddleware : CamMiddleware, IMHandler
    {
        private Transformer _origin;

        public bool Pre()
        {
            if (!SceneUtil.IsInMultiplayer)
            {
                Cam.TransformChain.Remove("MultiplayerOrigin");
                _origin = null;
            }
            /*
             * TODO: This should *eventually* (™) allow to set the origin of this camera to another player
             * which is not us, which would allow to have third and first person cameras which work in the
             * context of another player
             */
            var x = HookMultiplayerSpectatorController.instance;

            if (!SceneUtil.IsInSong || x == null || !Settings.Multiplayer.FollowSpectatorPlatform)
            {
                if (_origin == null)
                {
                    return true;
                }

                _origin.Position = Vector3.zero;
                _origin.Rotation = Quaternion.identity;

                return true;
            }

            _origin ??= Settings.Cam.TransformChain.AddOrGet("MultiplayerOrigin", TransformerOrders.PlayerOrigin);

            if (x.currentSpot == null)
            {
                return true;
            }

            _origin.Position = x.currentSpot.transform.position;
            _origin.Rotation = x.currentSpot.transform.rotation;

            return true;
        }

        public void Post() { }

        public void CamConfigReloaded() { }
    }
}