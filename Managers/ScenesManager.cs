using Camera2.Configuration;
using Camera2.HarmonyPatches;
using Camera2.SDK;
using Camera2.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Camera2.Enums;
using UnityEngine;

namespace Camera2.Managers
{
    internal static class ScenesManager
    {
        public static readonly HashSet<string> MenuSceneNames = new HashSet<string> { "MainMenu", "MenuViewCore", "MenuCore", "MenuViewControllers" };
        public static readonly HashSet<string> DisabledSceneNames = new HashSet<string> { "PCInit", "BeatmapEditor3D", "BeatmapLevelEditorWorldUi" };

        internal static ScenesSettings Settings { get; } = new ScenesSettings();

        // Kind of a hack not having it start off Menu but else the first menu load will not apply...
        internal static SceneTypes? LoadedScene { get; private set; }

        private static bool isOnCustomScene;

        public static void ActiveSceneChanged(string sceneName = null)
        {
            if (SceneUtil.CurrentScene == null)
            {
                return;
            }

            sceneName ??= SceneUtil.CurrentScene.name;

            if (CamManager.CustomScreen == null)
            {
                return;
            }

            if (!Settings.AutoswitchFromCustom && isOnCustomScene)
            {
                return;
            }

            SharedCoroutineStarter.instance.StartCoroutine(LoadGameSceneNextFrame(sceneName));
        }

        private static IEnumerator LoadGameSceneNextFrame(string sceneName = null)
        {
            yield return null;
            LoadGameScene(sceneName);
        }

        public static void LoadGameScene(string sceneName = null, bool forceReload = false)
        {
            sceneName ??= SceneUtil.CurrentScene.name;

            if (sceneName == "EmptyTransition" || sceneName == "ShaderWarmup" || sceneName == "ShaderWarmup")
            {
                return;
            }

            if (DisabledSceneNames.Contains(sceneName))
            {
                SwitchToCamList(null, false);
                LoadedScene = null;
                return;
            }

            //Plugin.Log.Info($"LoadGameScene: {sceneName}");
            
            var toLookup = new List<SceneTypes>(2) { SceneTypes.Menu };

            if (MenuSceneNames.Contains(sceneName))
            {
                if (SceneUtil.IsInMultiplayer)
                {
                    toLookup.Insert(0, SceneTypes.MultiplayerMenu);
                }
            }
            // added support for MultiplayerExtensions which adds a scene called MultiplayerEnvironment
            else if (sceneName == "GameCore" || sceneName == "MultiplayerEnvironment")
            {
                toLookup.Insert(0, SceneTypes.Playing);

                if (HookLeveldata.IsModdedMap)
                {
                    toLookup.Insert(0, SceneTypes.PlayingModmap);
                }
                else if (HookLeveldata.Is360Level)
                {
                    toLookup.Insert(0, SceneTypes.Playing360);
                }

                //Plugin.Log.Info("RS count: " + ReplaySources.Sources.Count);
                
                if (ReplaySources.Sources.Any(x => x.IsInReplay))
                {
                    toLookup.Insert(0, SceneTypes.Replay);
                }
                else if (SceneUtil.IsInMultiplayer)
                {
                    toLookup.Insert(0, SceneTypes.PlayingMulti);

                    if (HookMultiplayerSpectatorController.instance != null)
                    {
                        toLookup.Insert(0, SceneTypes.SpectatingMulti);
                    }
                }
            }

            if (HookFPFCToggle.isInFPFC)
            {
                toLookup.Insert(0, SceneTypes.FPFC);
            }

            //Plugin.Log.Info($"LoadGameScene -> {string.Join(", ", toLookup)}");

            SwitchToScene(FindSceneToUse(toLookup), forceReload);
        }

        public static void SwitchToScene(SceneTypes scene, bool forceReload = false)
        {
            if (!Settings.Scenes.ContainsKey(scene))
            {
                return;
            }

            Plugin.Log.Info($"Switching to scene {scene}");
            Plugin.Log.Info($"Cameras: {string.Join(", ", Settings.Scenes[scene])}");
            
            if (LoadedScene == scene && !forceReload && !isOnCustomScene)
            {
                return;
            }

            LoadedScene = scene;

            var toLoad = Settings.Scenes[scene];

            if (scene == SceneTypes.Menu && toLoad.Count == 0)
            {
                toLoad = CamManager.Cams.Select(x => x.Name).ToList();
            }

            SwitchToCamList(toLoad);
            isOnCustomScene = false;
            UI.SpaghettiUI.ScenesSwitchUI.Update(0, false);
        }

        public static void SwitchToCustomScene(string name)
        {
            if (!Settings.CustomScenes.TryGetValue(name, out var scene))
            {
                return;
            }

            if (scene.All(x => CamManager.GetCameraByName(x) == null))
            {
                return;
            }

            isOnCustomScene = true;

            SwitchToCamList(scene);
        }

        private static void SwitchToCamList(List<string> cams, bool activateAllWhenEmpty = true)
        {
            if (cams?.Count == 0)
            {
                cams = null;
            }

            /*
             * Intentionally checking != false, this way if cams is null OR
             * it contains it, the cam will be activated, only if it's
             * a non-empty scene we want to hide cams that are not in it
             */
            foreach (var cam in CamManager.Cams)
            {
                if (cam == null)
                {
                    continue;
                }

                var isContained = cams?.Contains(cam.Name);

                var camShouldBeActive = (activateAllWhenEmpty && isContained != false) || isContained == true || UI.CamSettings.CurrentCam == cam;

                cam.gameObject.SetActive(camShouldBeActive);
            }

            GL.Clear(true, true, Color.black);

            GlobalFPSCap.ApplyFPSCap();
        }

        private static SceneTypes FindSceneToUse(IEnumerable<SceneTypes> types)
        {
            return Settings.Scenes.Count == 0 
                ? SceneTypes.Menu 
                : types.FirstOrDefault(type => Settings.Scenes[type].Any(x => CamManager.GetCameraByName(x) != null));
        }
    }
}