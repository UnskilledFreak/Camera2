using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.MenuButtons;
using Camera2.Managers;
using HMUI;
using JetBrains.Annotations;

namespace Camera2.UI
{
    [UsedImplicitly]
    internal class NonSpaghettiUI
    {
        [UsedImplicitly]
        private static SettingsCoordinator settingsFlow;
        [UsedImplicitly]
        private static SceneCoordinator sceneFlow;
        //private static ScriptCoordinator scriptFlow;
        internal static readonly CustomScenesSwitchUI ScenesSwitchUI = new CustomScenesSwitchUI();

        public static void Init()
        {
            MenuButtons.instance.RegisterButton(new MenuButton(Plugin.Name, "Why helping others when you can be a jerk?", () => ShowFlow(settingsFlow)));
            MenuButtons.instance.RegisterButton(new MenuButton(Plugin.Name + " Scene Tester", "Test all scenes here", () => ShowFlow(sceneFlow)));
            //MenuButtons.instance.RegisterButton(new MenuButton(Plugin.Name + " Movement Scripts", "QoL is hard, isn't it?", () => ShowFlow(scriptFlow)));

            if (ScenesManager.Settings.CustomScenes.Count > 0)
            {
                GameplaySetup.instance.AddTab(Plugin.Name, "Camera2.UI.Views.customScenesList.bsml", ScenesSwitchUI);
            }
        }

        private static void ShowFlow<T>(T coordinator) where T : FlowCoordinator
        {
            if (coordinator == null)
            {
                coordinator = BeatSaberUI.CreateFlowCoordinator<T>();
            }
            
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(coordinator);
        }
    }
}