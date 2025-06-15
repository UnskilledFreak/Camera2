using Camera2.Managers;
using Camera2.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch(typeof(SmoothCameraController), nameof(SmoothCameraController.ActivateSmoothCameraIfNeeded))]
    internal static class InitOnMainAvailable
    {
        private static bool isInitialized;
#if PRE_1_40_6
        public static bool UseDepthTexture { get; private set; }
#endif

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
#if PRE_1_37_1
        private static void Postfix(MainSettingsModelSO ____mainSettingsModel)
        {
            UseDepthTexture = ____mainSettingsModel.smokeGraphicsSettings;

            if (!isInitialized)
            {
#elif V1_34_2
        private static void Postfix(SmoothCameraController __instance)
        {
            UseDepthTexture = false;
            if (!isInitialized)
            {
                if (SceneUtil.GetMainCameraButReally().GetComponent<DepthTextureController>()._handler.TryGetCurrentPerformancePreset(out var pp))
                {
                    UseDepthTexture = pp.smokeGraphics; 
                }
#else
        private static void Postfix(SmoothCameraController __instance)
        {
#if PRE_1_40_6
            UseDepthTexture = false;
#endif
            
            if (!isInitialized)
            {
#if PRE_1_40_6
                UseDepthTexture = SceneUtil.GetMainCameraButReally().GetComponent<DepthTextureController>()._settingsManager.settings.quality.smokeGraphics;
#endif
#endif
                if (CamManager.BaseCullingMask == 0)
                {
                    CamManager.BaseCullingMask = Camera.main!.cullingMask;
                }

                isInitialized = true;

                Plugin.Log.Notice("Game is ready, Initializing...");

                CamManager.Init();
            }
            else
            {
                foreach (var cam in CamManager.Cams)
                {
                    cam.UpdateDepthTextureActive();
                }
            }
        }
    }
}