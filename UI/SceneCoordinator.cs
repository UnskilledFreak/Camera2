using BeatSaberMarkupLanguage;
using Camera2.Enums;
using Camera2.HarmonyPatches;
using Camera2.Managers;
using HMUI;

namespace Camera2.UI
{
    public class SceneCoordinator : FlowCoordinator
    {
        internal static SceneCoordinator Instance { get; private set; }
        internal SceneView SceneView;

        public void Awake()
        {
            Instance = this;
            
            if (SceneView == null)
            {
                SceneView = BeatSaberUI.CreateViewController<SceneView>();
            }
        }

#if V1_29_1
        public 
#else
        protected
#endif
            override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            showBackButton = true;
            SetTitle(Plugin.Name + " Scene Tester");

            ProvideInitialViewControllers(SceneView);
        }

        
#if V1_29_1
        public 
#else
        protected
#endif
            override void BackButtonWasPressed(ViewController thisTopViewController)
        {
            ScenesManager.SwitchToScene(HookFPFCToggle.isInFPFC ? SceneTypes.FPFC : SceneTypes.Menu);
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}