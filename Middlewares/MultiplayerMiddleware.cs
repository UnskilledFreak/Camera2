using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using UnityEngine;

namespace Camera2.Middlewares
{
    internal class MultiplayerMiddleware : CamMiddleware, IMHandler
    {
        public bool Pre()
        {
            if (!SceneUtil.IsInMultiplayer)
            {
                RemoveTransformer(TransformerTypeAndOrder.PlayerOrigin);
            }
            /*
             * TODO: This should *eventually* (™) allow to set the origin of this camera to another player
             * which is not us, which would allow to have third and first person cameras which work in the
             * context of another player
             */
            var x = HookMultiplayerSpectatorController.instance;

            if (!SceneUtil.IsInSong || x == null || !Settings.Multiplayer.FollowSpectatorPlatform)
            {
                if (Transformer == null)
                {
                    return true;
                }

                Transformer.Position = Vector3.zero;
                Transformer.Rotation = Quaternion.identity;

                return true;
            }

            AddTransformer(TransformerTypeAndOrder.PlayerOrigin);

            if (x.currentSpot == null)
            {
                return true;
            }

            Transformer.Position = x.currentSpot.transform.position;
            Transformer.Rotation = x.currentSpot.transform.rotation;

            return true;
        }

        public void Post() { }

        public void CamConfigReloaded() { }
    }
}