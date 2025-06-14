using Camera2.Managers;
using Camera2.Utils;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Camera2.HarmonyPatches
{
    /*
     * Eh, this is picked at random. OnActiveSceneChanged is too early for RecorderCamera to exit and
     * I cba to dig through obfuscated code because obscurity = security right?
     */
    //[HarmonyPatch(typeof(PauseMenuManager))]
    //[HarmonyPatch("Awake")]
    [HarmonyPatch(typeof(AudioTimeSyncController), nameof(AudioTimeSyncController.StartSong))]
    internal static class HookAudioTimeSyncController
    {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(AudioTimeSyncController __instance)
        {
#if DEBUG
            Plugin.Log.Info("AudioTimeSyncController.StartSong()");
#endif
            SceneUtil.SongStarted(__instance);
        }
    }

    [HarmonyPatch]
    internal static class HookAudioTimeSyncController2
    {
#if !V1_29_1
        [HarmonyPatch(typeof(AudioTimeSyncController), nameof(AudioTimeSyncController.Pause))]
        [HarmonyPatch(typeof(AudioTimeSyncController), nameof(AudioTimeSyncController.Resume))]
#endif
        [UsedImplicitly]
        private static void Postfix()
        {
#if DEBUG
            Plugin.Log.Info("AudioTimeSyncController.Pause/Resume()");
#endif
            CamManager.ApplyCameraValues(worldCam: true);
        }

#if V1_29_1
        [UsedImplicitly]
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(AudioTimeSyncController), nameof(AudioTimeSyncController.Pause));
            yield return AccessTools.Method(typeof(AudioTimeSyncController), nameof(AudioTimeSyncController.Resume));
        }
#endif
    }
}