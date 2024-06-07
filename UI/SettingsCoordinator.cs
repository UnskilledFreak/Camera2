using System;
using System.Linq;
using BeatSaberMarkupLanguage;
using Camera2.Behaviours;
using Camera2.Managers;
using HMUI;

namespace Camera2.UI
{
    internal class SettingsCoordinator : FlowCoordinator
    {
        internal static SettingsCoordinator Instance { get; private set; }

        internal SettingsView SettingsView;
        internal CamList CamList;
        private PreviewView _previewView;
        private Cam2 _lastSelected;

        public void Awake()
        {
            Instance = this;

            if (CamList == null)
            {
                CamList = BeatSaberUI.CreateViewController<CamList>();
            }

            if (SettingsView == null)
            {
                SettingsView = BeatSaberUI.CreateViewController<SettingsView>();
            }

            if (_previewView == null)
            {
                _previewView = BeatSaberUI.CreateViewController<PreviewView>();
            }
        }

        public void ShowSettingsForCam(Cam2 cam, bool reselect = false)
        {
            _lastSelected = cam;
            SetTitle($"{Plugin.Name} | {cam.Name}");

            if (!SettingsView.SetCam(cam) && !reselect)
            {
                return;
            }

            var cellIndex = Array.FindIndex(CamList.ListDataOrdered.ToArray(), x => x.Cam == cam);

            CamList.list.tableView.SelectCellWithIdx(cellIndex);
            CamList.list.tableView.ScrollToCellWithIdx(cellIndex, TableView.ScrollPositionType.Center, false);
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            try
            {
                if (!firstActivation)
                {
                    if (CamList != null)
                    {
                        CamList.Init();
                    }

                    ShowSettingsForCam(_lastSelected);
                    return;
                }

                showBackButton = true;

                ProvideInitialViewControllers(SettingsView, CamList, null, _previewView);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex);
            }
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            SettingsView.SetCam(null);
            ScenesManager.LoadGameScene(forceReload: true);
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }
    }
}