using Camera2.Managers;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch(typeof(SmoothCameraController), nameof(SmoothCameraController.ActivateSmoothCameraIfNeeded))]
    internal static class InitOnMainAvailable
    {
        private static bool isInitialized;
        public static bool UseDepthTexture { get; private set; }

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(MainSettingsModelSO ____mainSettingsModel)
        {
            UseDepthTexture = ____mainSettingsModel.smokeGraphicsSettings;

            if (!isInitialized)
            {
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