using Camera2.Managers;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch(typeof(SmoothCameraController), nameof(SmoothCameraController.ActivateSmoothCameraIfNeeded))]
    internal static class InitOnMainAvailable
    {
        static bool isInited = false;
        public static bool useDepthTexture { get; private set; }

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static void Postfix(MainSettingsModelSO ____mainSettingsModel)
        {
            useDepthTexture = ____mainSettingsModel.smokeGraphicsSettings;

            if (!isInited)
            {
                if (CamManager.BaseCullingMask == 0)
                {
                    CamManager.BaseCullingMask = Camera.main.cullingMask;
                }

                isInited = true;

                Plugin.Log.Notice("Game is ready, Initializing...");

                CamManager.Init();
            }
            else
            {
                foreach (var cam in CamManager.Cams.Values)
                {
                    cam.UpdateDepthTextureActive();
                }
            }
        }
    }
}