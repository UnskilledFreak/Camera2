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

        internal CamSettings CamSettings;
        internal CamList CamList;
        internal CamMovementSettings CamMovementSettings;
        private PreviewView _previewView;
        private Cam2 _lastSelected;
        private bool _isActive;

        public void Awake()
        {
            Instance = this;

            if (CamList == null)
            {
                CamList = BeatSaberUI.CreateViewController<CamList>();
            }

            if (CamSettings == null)
            {
                CamSettings = BeatSaberUI.CreateViewController<CamSettings>();
            }

            if (_previewView == null)
            {
                _previewView = BeatSaberUI.CreateViewController<PreviewView>();
            }

            if (CamMovementSettings == null)
            {
                CamMovementSettings = BeatSaberUI.CreateViewController<CamMovementSettings>();
            }
        }

        public void UpdateTitle(Cam2 cam)
        {
            SetTitle($"{Plugin.Name} | {cam.Name}");
            _lastSelected = cam;
        }

        public void ShowSettingsForCam(Cam2 cam, bool reSelect = false)
        {
            if (!_isActive)
            {
                return;
            }

            if (!CamSettings.SetCam(cam) && !reSelect)
            {
                return;
            }

            UpdateTitle(cam);
            CamMovementSettings.SetCam(cam);

            var cellIndex = Array.FindIndex(CamList.ListDataOrdered.ToArray(), x => x.Cam.Name == cam.Name);

            CamList.list.tableView.SelectCellWithIdx(cellIndex);
            CamList.list.tableView.ScrollToCellWithIdx(cellIndex, TableView.ScrollPositionType.Center, false);

            if (reSelect)
            {
                CamSettings.ReselectLastTab();
            }
        }


#if V1_29_1
        public
#else
        protected
#endif
            override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            _isActive = true;
            try
            {
                if (!firstActivation)
                {
                    if (CamList != null)
                    {
                        CamList.Init();
                    }

                    if (_lastSelected != null)
                    {
                        ShowSettingsForCam(_lastSelected);
                    }

                    return;
                }

                showBackButton = true;

                ProvideInitialViewControllers(CamSettings, CamList, CamMovementSettings, _previewView);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex);
            }
        }


#if V1_29_1
        public
#else
        protected
#endif
            override void BackButtonWasPressed(ViewController topViewController)
        {
            CamSettings.SetCam(null);
            CamMovementSettings.SetCam(null);
            ScenesManager.LoadGameScene(forceReload: true);
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
            _isActive = false;
        }
    }
}