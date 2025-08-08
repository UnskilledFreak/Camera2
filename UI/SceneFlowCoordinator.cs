using BeatSaberMarkupLanguage;
using Camera2.Enums;
using Camera2.HarmonyPatches;
using Camera2.Managers;
using HMUI;
using Zenject;

namespace Camera2.UI
{
    internal class SceneFlowCoordinator : FlowCoordinator
    {
        private MainFlowCoordinator _mainFlowCoordinator;
        private SceneViewController _sceneViewController;

        [Inject]
        public void Construct(
            MainFlowCoordinator mainFlowCoordinator,
            SceneViewController sceneViewController
            )
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _sceneViewController = sceneViewController;
        }

#if V1_29_1
        public 
#else
        protected
#endif
            override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                showBackButton = true;
                ProvideInitialViewControllers(_sceneViewController);
            }

            SetTitle(Plugin.Name + " Scene Tester");
        }

        
#if V1_29_1
        public 
#else
        protected
#endif
            override void BackButtonWasPressed(ViewController thisTopViewController)
        {
            ScenesManager.SwitchToScene(HookFPFCToggle.IsInFpfc ? SceneTypes.FPFC : SceneTypes.Menu);
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}