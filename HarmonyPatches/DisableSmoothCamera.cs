using HarmonyLib;
using JetBrains.Annotations;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch(typeof(SmoothCamera), nameof(SmoothCamera.OnEnable))]
    internal static class DisableSmoothCamera
    {
        [UsedImplicitly]
        private static bool Prefix()
        {
#if DEBUG
            Plugin.Log.Info("Prevented Smooth camera from activating");
#endif
            return false;
        }
    }
}