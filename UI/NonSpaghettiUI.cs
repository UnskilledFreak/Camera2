using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using JetBrains.Annotations;

namespace Camera2.UI
{
    [UsedImplicitly]
    internal class NonSpaghettiUI
    {
        private static readonly Dictionary<Type, FlowCoordinator> Coordinators = new Dictionary<Type, FlowCoordinator>();
        internal static readonly CustomScenesSwitchUI ScenesSwitchUI = new CustomScenesSwitchUI();

        public static void Init()
        {
            Console.WriteLine("MenuButtons? " + (MenuButtons.instance == null ? "NULL!" : "sdfsdfs"));
            MenuButtons.instance.RegisterButton(new MenuButton(Plugin.Name, "Why helping others when you can be a jerk?", ShowFlow<SettingsCoordinator>));
            MenuButtons.instance.RegisterButton(new MenuButton(Plugin.Name + " Scene Tester", "Test all scenes here", ShowFlow<SceneCoordinator>));
            //MenuButtons.instance.RegisterButton(new MenuButton(Plugin.Name + " Movement Scripts", "QoL is hard, isn't it?", ShowFlow<ScriptFlowCoordinator>));

            GameplaySetup.instance.AddTab(Plugin.Name, "Camera2.UI.Views.customScenesList.bsml", ScenesSwitchUI);
        }

        private static void ShowFlow<T>() where T: FlowCoordinator
        {
            var type = typeof(T);
            if (!Coordinators.ContainsKey(type))
            {
                Coordinators.Add(type, BeatSaberUI.CreateFlowCoordinator<T>());
            }
            
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(Coordinators[type]);
        }
    }
}