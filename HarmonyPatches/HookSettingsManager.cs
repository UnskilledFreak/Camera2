using HarmonyLib;

namespace Camera2.HarmonyPatches;
#if !PRE_1_40_6
[HarmonyPatch(typeof(DepthTextureController), nameof(DepthTextureController.Init))]
internal static class HookSettingsManager
{
    public static SettingsManager SettingsManager { get; private set; }
    public static bool UseDepthTexture { get; private set; }

    private static void Postfix(SettingsManager settingsManager)
    {
        HookSettingsManager.SettingsManager = settingsManager;
        UseDepthTexture = settingsManager.settings.quality.smokeGraphics;
    }
}
#endif