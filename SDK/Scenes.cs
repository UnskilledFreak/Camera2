using Camera2.Managers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Camera2.Enums;
using JetBrains.Annotations;

namespace Camera2.SDK
{
    public static class Scenes
    {
        /// <summary>
        /// List of Scenes and what cameras belong to them
        /// </summary>
        [UsedImplicitly]
        public static IReadOnlyDictionary<SceneTypes, ReadOnlyCollection<string>> AllScenes => ScenesManager.Settings.Scenes.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly());

        /// <summary>
        /// List of Scenes and what cameras belong to them
        /// </summary>
        [UsedImplicitly]
        public static IReadOnlyDictionary<string, ReadOnlyCollection<string>> CustomScenes => ScenesManager.Settings.CustomScenes.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly());

        /// <summary>
        /// The currently loaded scene. Must not necessarily represent the targeted scene
        /// (E.g. Playing) incase the Playing scene was empty and Cam2 resorted to using Menu
        /// instead, etc.
        /// </summary>
        [UsedImplicitly]
        public static SceneTypes Current => ScenesManager.LoadedScene ?? SceneTypes.Menu;

        /// <summary>
        /// Switches to the requested custom scene. If the scene you try to switch to does not have
        /// any cameras assigned no action will be taken.
        /// </summary>
        /// <param name="sceneName">Scene to switch to</param>
        [UsedImplicitly]
        public static void SwitchToCustomScene(string sceneName)
        {
            ScenesManager.SwitchToCustomScene(sceneName);
        }

        /// <summary>
        /// Switches to whatever scene *should* be active right now, assuming it is overriden
        /// </summary>
        [UsedImplicitly]
        public static void ShowNormalScene()
        {
            ScenesManager.LoadGameScene(forceReload: true);
        }

        /// <summary>
        /// reloads the config like the CTRL + SHIFT + F1 hotkey does
        /// </summary>
        [UsedImplicitly]
        public static void ReloadConfig()
        {
            CamManager.CompleteReload();
        }

        /// <summary>
        /// gets or sets the auto switch from custom scene value and reloads config because that's missing...
        /// </summary>
        [UsedImplicitly]
        public static bool AutoSwitchFromCustomScene
        {
            get => ScenesManager.Settings.AutoSwitchFromCustom;
            set
            {
                ScenesManager.Settings.AutoSwitchFromCustom = value;
                ScenesManager.Settings.Save();
                CamManager.CompleteReload();
            }
        }
    }
}