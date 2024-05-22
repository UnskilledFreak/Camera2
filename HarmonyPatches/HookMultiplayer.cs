using Camera2.Managers;
using HarmonyLib;
using JetBrains.Annotations;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch(typeof(MultiplayerSessionManager), "UpdateConnectionState")]
    internal static class HookMultiplayer
    {
        private static MultiplayerSessionManager _instance;
        public static MultiplayerSessionManager instance => _instance == null ? null : _instance;

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(MultiplayerSessionManager __instance)
        {
#if DEBUG
            Plugin.Log.Info($"Multiplayer connection state changed. Connected: {__instance.isConnected}");
#endif
            _instance = __instance;
            ScenesManager.ActiveSceneChanged();
        }
    }

    [HarmonyPatch(typeof(MultiplayerSpectatorController), nameof(MultiplayerSpectatorController.Start))]
    internal static class HookMultiplayerSpectatorController
    {
        private static MultiplayerSpectatorController _instance;
        public static MultiplayerSpectatorController instance => _instance == null || !_instance.isActiveAndEnabled ? null : _instance;

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(MultiplayerSpectatorController __instance)
        {
#if DEBUG
            Plugin.Log.Info($"MultiplayerSpectatorController.Start()");
#endif
            _instance = __instance;
            ScenesManager.ActiveSceneChanged();
        }
    }
}