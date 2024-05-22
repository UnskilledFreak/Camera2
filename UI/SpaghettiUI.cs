using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.MenuButtons;
using Camera2.Managers;

namespace Camera2.UI
{
    // There's a reason this is called Spaghetti UI, I will definitely maybe possibly make this not spaghetti one day.
    internal class SpaghettiUI
    {
        private static Coordinator flow;
        internal static readonly CustomScenesSwitchUI ScenesSwitchUI = new CustomScenesSwitchUI();

        public static void Init()
        {
            MenuButtons.instance.RegisterButton(new MenuButton(Plugin.Name, "Why helping others when you can be a jerk?", ShowFlow));

            if (ScenesManager.Settings.CustomScenes.Count > 0)
            {
                GameplaySetup.instance.AddTab(Plugin.Name, "Camera2.UI.Views.customScenesList.bsml", ScenesSwitchUI);
            }
        }

        private static void ShowFlow()
        {
            if (flow == null)
            {
                flow = BeatSaberUI.CreateFlowCoordinator<Coordinator>();
            }

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(flow);
        }
    }
}