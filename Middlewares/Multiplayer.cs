using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Utils;
using UnityEngine;

namespace Camera2.Middlewares
{
    internal class Multiplayer : CamMiddleware, IMHandler
    {
        private Transformer _originLmao;

        public new bool Pre()
        {
            /*
             * TODO: This should *eventually* (™) allow to set the origin of this camera to another player
             * which is not us, which would allow to have third and firstperson cameras which work in the
             * context of another player
             */
            var x = HookMultiplayerSpectatorController.instance;

            if (!SceneUtil.IsInMultiplayer || !SceneUtil.IsInSong || x == null || !Settings.Multiplayer.FollowSpectatorPlatform)
            {
                if (_originLmao == null)
                {
                    return true;
                }

                _originLmao.Position = Vector3.zero;
                _originLmao.Rotation = Quaternion.identity;

                return true;
            }

            _originLmao ??= Settings.Cam.TransformChain.AddOrGet("MultiplayerOrigin", TransformerOrders.PlayerOrigin);

            if (x.currentSpot == null)
            {
                return true;
            }

            _originLmao.Position = x.currentSpot.transform.position;
            _originLmao.Rotation = x.currentSpot.transform.rotation;

            return true;
        }
    }
}