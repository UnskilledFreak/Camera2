using Camera2.HarmonyPatches;
using Camera2.Managers;
using System.Linq;
using Camera2.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Camera2.Utils
{
    internal static class SceneUtil
    {
        public static Scene CurrentScene { get; private set; }
        public static bool IsInMenu { get; private set; } = true;
        public static bool IsInSong { get; private set; }
        public static AudioTimeSyncController AudioTimeSyncController { get; private set; }
        public static bool HasSongPlayer => AudioTimeSyncController != null;
        public static bool IsSongPlaying => HasSongPlayer && AudioTimeSyncController.state == AudioTimeSyncController.State.Playing;
        public static bool IsInMultiplayer => HookMultiplayer.instance != null && HookMultiplayer.instance.isConnected;

        public static GameObject GetMainCameraButReally()
        {
            var a = Camera.main;
            return a == null
                ? GameObject.FindGameObjectsWithTag("MainCamera")[0]
                : a.gameObject;
        }


        public static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            CurrentScene = newScene;
            IsInSong = CurrentScene.name == "GameCore";
            IsInMenu = !IsInSong && ScenesManager.MenuSceneNames.Contains(CurrentScene.name);

            // added support for MultiplayerExtensions which adds a scene called MultiplayerEnvironment
            if (oldScene.name == "GameCore" || oldScene.name == "MultiplayerEnvironment")
            {
                ScoreSaber.IsInReplayProp = false;
                AudioTimeSyncController = null;
            }

            if (CurrentScene.name == "MainMenu")
            {
                HookLeveldata.Reset();
            }

            ScenesManager.ActiveSceneChanged();

            if (CamManager.CustomScreen != null)
            {
                CamManager.CustomScreen.gameObject.SetActive(!ScenesManager.DisabledSceneNames.Contains(CurrentScene.name));
            }

            // Updating the bitmask on scene change to allow for things like the auto wall toggle
            CamManager.ApplyCameraValues(bitMask: true, worldCam: true, posRot: true);

            GlobalFPSCap.ApplyFPSCap();
        }

        public static void SongStarted(AudioTimeSyncController controller)
        {
            AudioTimeSyncController = controller;
            ScoreSaber.UpdateIsInReplay();

            TransparentWalls.MakeWallsOpaqueForMainCam();
            CamManager.ApplyCameraValues(worldCam: true);

            if (CamManager.Cams.Values.All(x => x.Settings.VisibleObjects.Floor))
            {
                return;
            }

            // Move the platform stuff to the correct layer because beat games didnt
            foreach (var x in new[] { "Construction", "Frame", "RectangleFakeGlow" }.Select(x => GameObject.Find($"Environment/PlayersPlace/{x}")))
            {
                if (x != null)
                {
                    x.layer = (int)VisibilityLayers.PlayerPlatform;
                }
            }
        }
    }
}