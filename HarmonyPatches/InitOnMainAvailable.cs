using Camera2.Managers;
using Camera2.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.HarmonyPatches
{// TODO: Change to .depthTexture when they fixed that its true even with smoke off
    [HarmonyPatch(typeof(SmoothCameraController), nameof(SmoothCameraController.ActivateSmoothCameraIfNeeded))]
    internal static class InitOnMainAvailable
    {
        private static bool isInitialized;
        public static bool UseDepthTexture { get; private set; }

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
#if PRE_1_37_1
        private static void Postfix(MainSettingsModelSO ____mainSettingsModel)
        {
            UseDepthTexture = ____mainSettingsModel.smokeGraphicsSettings;

            if (!isInitialized)
            {
#else
        private static void Postfix(SmoothCameraController __instance)
        {
            UseDepthTexture = false;
            if (!isInitialized)
            {
                if (SceneUtil.GetMainCameraButReally().GetComponent<DepthTextureController>()._handler.TryGetCurrentPerformancePreset(out var pp))
                {
                    // TODO: Change to .depthTexture when they fixed that its true even with smoke off
                    UseDepthTexture = pp.smokeGraphics; 
                }
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