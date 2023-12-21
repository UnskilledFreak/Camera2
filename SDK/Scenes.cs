using Camera2.Configuration;
using Camera2.Managers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Camera2.Behaviours;

namespace Camera2.SDK {
	public static class Scenes {
		/// <summary>
		/// List of Scenes and what cameras belong to them
		/// </summary>
		public static IReadOnlyDictionary<SceneTypes, ReadOnlyCollection<string>> scenes =>
			ScenesManager.settings.scenes.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly());

		/// <summary>
		/// List of Scenes and what cameras belong to them
		/// </summary>
		public static IReadOnlyDictionary<string, ReadOnlyCollection<string>> customScenes =>
			ScenesManager.settings.customScenes.ToDictionary(pair => pair.Key, pair => pair.Value.AsReadOnly());

		/// <summary>
		/// The currently loaded scene. Must not necessarily represent the targeted scene
		/// (E.g. Playing) incase the Playing scene was empty and Cam2 resorted to using Menu
		/// instead, etc.
		/// </summary>
		public static SceneTypes current => ScenesManager.loadedScene ?? SceneTypes.Menu;

		/// <summary>
		/// Switches to the requested custom scene. If the scene you try to switch to does not have
		/// any cameras assigned no action will be taken.
		/// </summary>
		/// <param name="scene">Scene to switch to</param>
		public static void SwitchToCustomScene(string sceneName) {
			ScenesManager.SwitchToCustomScene(sceneName);
		}

		/// <summary>
		/// Switches to whatever scene *should* be active right now, assuming it is overriden
		/// </summary>
		public static void ShowNormalScene() {
			ScenesManager.LoadGameScene(forceReload: true);
		}

		/// <summary>
		/// reloads the config like the CTRL + SHIFT + F1 hotkey does
		/// </summary>
		public static void ReloadConfig() {
			CamManager.CompleteReload();
		}

		/// <summary>
		/// gets or sets the auto switch from custom scene value and reloads config because that's missing...
		/// </summary>
		public static bool AutoSwitchFromCustomScene {
			get => ScenesManager.settings.autoswitchFromCustom;
			set {
				ScenesManager.settings.autoswitchFromCustom = value;
				ScenesManager.settings.Save();
				CamManager.CompleteReload();
			}
		}
	}
}
