using System;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using Zenject;

namespace Camera2.UI;

// ReSharper disable once ClassNeverInstantiated.Global
internal class MenuButtonManager : IInitializable, IDisposable
{
    private readonly MainFlowCoordinator _mainFlowCoordinator;
    private readonly SettingsFlowCoordinator _settingsFlowCoordinator;
    private readonly SceneFlowCoordinator _sceneFlowCoordinator;
    private readonly MenuButtons _menuButtons;
    private readonly MenuButton _menuButtonMain;
    private readonly MenuButton _menuButtonScene;
    //private readonly MenuButton _menuButtonScript;
    
    internal static readonly CustomScenesSwitchUI ScenesSwitchUI = new CustomScenesSwitchUI();
    
#if !PRE_1_37_1
    private static MenuButton _instance;
#endif

#if V1_29_1
    private MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, SettingsFlowCoordinator settingsFlowCoordinator, SceneFlowCoordinator sceneFlowCoordinator)
#else
    private MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, SettingsFlowCoordinator settingsFlowCoordinator, SceneFlowCoordinator sceneFlowCoordinator, MenuButtons menuButtons)
#endif
    {
        _mainFlowCoordinator = mainFlowCoordinator;
        _settingsFlowCoordinator = settingsFlowCoordinator;
        _sceneFlowCoordinator = sceneFlowCoordinator;
#if V1_29_1
        _menuButtons = MenuButtons.instance;
#else
        _menuButtons = menuButtons;
#endif

        _menuButtonMain = new MenuButton(Plugin.Name, "Why helping others when you can be a jerk?", () => ShowFlow(_settingsFlowCoordinator));
        _menuButtonScene = new MenuButton(Plugin.Name + " Scene Tester", "Test all scenes here", () => ShowFlow(_sceneFlowCoordinator));
        //_menuButtonScript = new MenuButton(Plugin.Name + " Movement Scripts", "QoL is hard, isn't it?", () => ShowFlow(_scriptFlowCoordinator))

#if !PRE_1_37_1
        _instance = _menuButtonMain;
#endif
    }

    public void Initialize()
    {
#if PRE_1_37_1
        MenuButtons.instance.RegisterButton(_menuButtonMain);
        MenuButtons.instance.RegisterButton(_menuButtonScene);
        //MenuButtons.instance.RegisterButton(_menuButtonScript);
#else
        _menuButtons.RegisterButton(_menuButtonMain);
        _menuButtons.RegisterButton(_menuButtonScene);
        //_menuButtons.RegisterButton(_menuButtonScript);
#endif
#if PRE_1_40_8
        GameplaySetup.instance
#else
        GameplaySetup.Instance
#endif
            .AddTab(Plugin.Name, "Camera2.UI.Views.CustomScenesList.bsml", ScenesSwitchUI);
    }

    public void Dispose()
    {
        _menuButtons.UnregisterButton(_menuButtonMain);
        _menuButtons.UnregisterButton(_menuButtonScene);
        //_menuButtons.UnregisterButton(_menuButtonScript);
        
#if PRE_1_40_8
        GameplaySetup.instance
#else
        GameplaySetup.Instance
#endif
            .RemoveTab(Plugin.Name);
    }

    private void ShowFlow(FlowCoordinator flowCoordinator)
    {
        _mainFlowCoordinator.PresentFlowCoordinator(flowCoordinator);
    }
}