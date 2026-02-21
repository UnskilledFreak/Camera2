using Camera2.Managers;
using HarmonyLib;
using JetBrains.Annotations;

namespace Camera2.HarmonyPatches
{
#if PRE_1_42_2
    [HarmonyPatch(typeof(MultiplayerSessionManager), "UpdateConnectionState")]
#endif
    internal static class HookMultiplayer
    {
#if PRE_1_42_2
        private static MultiplayerSessionManager _instance;
        public static MultiplayerSessionManager instance => _instance == null ? null : _instance;
#else
        private static MultiplayerModeSelectionFlowCoordinator _instance;
        public static MultiplayerModeSelectionFlowCoordinator instance => _instance?._lobbyGameStateController == null ? null : _instance;
#endif


        [UsedImplicitly]
#if PRE_1_42_2
        // ReSharper disable once InconsistentNaming
        private static void Postfix(MultiplayerSessionManager __instance)
#else
        [HarmonyPatch(typeof(MultiplayerModeSelectionFlowCoordinator), nameof(MultiplayerModeSelectionFlowCoordinator.TransitionDidFinish))]
        private static void Postfix(MultiplayerModeSelectionFlowCoordinator __instance)
#endif

        {
#if DEBUG
#if PRE_1_42_2
            Plugin.Log.Info($"Multiplayer connection state changed. Connected: {__instance.isConnected}");
#else
            Plugin.Log.Info($"Multiplayer connection state changed. Connected: {__instance._lobbyGameStateController?.state}");
#endif
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
            Plugin.Log.Info("MultiplayerSpectatorController.Start()");
#endif
            _instance = __instance;
            ScenesManager.ActiveSceneChanged();
        }
    }
}