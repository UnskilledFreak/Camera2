using HarmonyLib;
using JetBrains.Annotations;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch(typeof(EnvironmentSpawnRotation), nameof(EnvironmentSpawnRotation.OnEnable))]
    internal static class HookLevelRotation
    {
        public static EnvironmentSpawnRotation Instance { get; private set; }

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(EnvironmentSpawnRotation __instance)
        {
#if DEBUG
            Plugin.Log.Info("Got EnvironmentSpawnRotation instance");
#endif
            Instance = __instance;
        }
    }
}