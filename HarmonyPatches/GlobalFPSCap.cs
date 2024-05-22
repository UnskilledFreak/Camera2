using Camera2.Managers;
using HarmonyLib;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.HarmonyPatches
{
    [HarmonyPatch(typeof(MainSystemInit), nameof(MainSystemInit.Init))]
    internal static class GlobalFPSCap
    {
        private static bool isOculus;
        private static bool isOculusUserPresent;
        
        public static void Init()
        {
            /*
             * On VRMode Oculus, when you take off the headset the game ends up in an uncapped FPS state,
             * this makes sure to apply an FPS cap when the headset is taken off
             */
            if (!OVRPlugin.initialized)
            {
                return;
            }

            isOculus = true;

            Task.Run(delegate
            {
                for (;;)
                {
                    var newPresentState = OVRPlugin.userPresent;

                    if (newPresentState != isOculusUserPresent)
                    {
#if DEBUG
                        Plugin.Log.Info(newPresentState ? "HMD mounted - Removing FPS cap" : "HMD unmounted - Applying FPS cap");
#endif
                        ApplyFPSCap(newPresentState);

                        isOculusUserPresent = newPresentState;
                    }

                    System.Threading.Thread.Sleep(isOculusUserPresent ? 2000 : 750);
                }
            });
        }

        public static void ApplyFPSCap()
        {
            ApplyFPSCap(UnityEngine.XR.XRDevice.isPresent || UnityEngine.XR.XRDevice.refreshRate != 0);
        }

        [UsedImplicitly]
        private static void Postfix() => ApplyFPSCap();

        private static void ApplyFPSCap(bool isHmdPresent)
        {
            QualitySettings.vSyncCount = 0;

            if (isHmdPresent && (!isOculus || isOculusUserPresent))
            {
                Application.targetFrameRate = -1;
            }
            else
            {
                var cap = 30;

                if (CamManager.Cams?.Count > 0)
                {
                    QualitySettings.vSyncCount = 1;
                    var srr = Screen.currentResolution.refreshRate;
                    cap = -1;

                    foreach (var cam in CamManager.Cams.Values.Where(x => x.gameObject.activeInHierarchy))
                    {
                        if (cam.Settings.FPSLimiter.FPSLimit <= 0 || cam.Settings.FPSLimiter.FPSLimit == srr)
                        {
                            if (cap < srr)
                            {
                                cap = srr;
                            }
                        }
                        else if (cap < cam.Settings.FPSLimiter.FPSLimit)
                        {
                            cap = cam.Settings.FPSLimiter.FPSLimit;
                            QualitySettings.vSyncCount = 0;
                        }
                    }
                }

                Application.targetFrameRate = cap;
            }
        }
    }
}