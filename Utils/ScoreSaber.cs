using System.Reflection;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using UnityEngine;

namespace Camera2.Utils
{
    internal static class ScoreSaber
    {
        public class ScoreSaberReplaySource : ISource
        {
            public string Name => "ScoreSaber";
            public bool IsInReplay => IsInReplayFunc();
            public Vector3 localHeadPosition => ReplayCamera == null ? Vector3.zero : ReplayCamera.transform.localPosition;
            public Quaternion localHeadRotation => ReplayCamera == null ? Quaternion.identity : ReplayCamera.transform.localRotation;
        }

        public static bool IsInReplayProp { get; internal set; }
        public static Transform SpectateParent { get; private set; }

        private static Camera ReplayCamera { get; set; }

        private static MethodBase scoreSaberPlaybackEnabled;

        public static bool Reflect()
        {
            return (scoreSaberPlaybackEnabled = IPA.Loader.PluginManager
                    .GetPluginFromId("ScoreSaber")
                    ?.Assembly
                    .GetType("ScoreSaber.Core.ReplaySystem.HarmonyPatches.PatchHandleHMDUnmounted")
                    ?.GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic)
                ) != null;
        }

        public static void UpdateIsInReplay()
        {
            var wasInReplay = IsInReplayProp;

            IsInReplayProp = IsInReplayFunc();
            ReplayCamera = !IsInReplayProp ? null : GameObject.Find("LocalPlayerGameCore/Recorder/RecorderCamera")?.GetComponent<Camera>();

            if (ReplayCamera != null)
            {
                var x = GameObject.Find("RecorderCamera(Clone)")?.GetComponent<Camera>();

                // Cant disable this one as otherwise SS' ReplayFrameRenderer stuff "breaks"
                //replayCamera.enabled = false;
                if (x != null)
                {
                    ReplayCamera.tag = "Untagged";

                    /*
                     * When a replay was just started, the VRCenterAdjust isn't set up "correctly" and Cam2 has no idea about
                     * the offset applied for the VR-Spectator. This is mainly relevant when having "Follow replay position"
                     * off as then the camera would be too far forward, assuming default settings, until you open the Pause
                     * menu or the Replay UI ingame
                     * This is super hacky trash, but it does the job for now™
                     * I'm not exactly sure why I *need* to look it up again here, else it won't work - whatever.
                     */
                    if (!wasInReplay)
                    {
                        var y = GameObject.Find("SpectatorParent/RecorderCamera(Clone)");

                        SpectateParent = y == null ? null : y.transform.parent;
                    }
#if V1_29_1
                    if (!UnityEngine.XR.XRDevice.isPresent)
#else
                    if (GlobalFPSCap.GetActiveVrDevice() == null)
#endif
                    
                    {
                        x.enabled = false;
                    }

                    // Doing this so other plugins that rely on Camera.main don't die
                    x.tag = "MainCamera";
                }
            }

            Plugin.Log.Info($"UpdateIsInReplay() -> isInReplay: {IsInReplayProp}, replayCamera: {ReplayCamera}");
        }

        private static bool IsInReplayFunc()
        {
            try
            {
                return scoreSaberPlaybackEnabled != null && !(bool)scoreSaberPlaybackEnabled.Invoke(null, null);
            }
            catch
            {
                // ignored
            }

            return false;
        }
    }
}