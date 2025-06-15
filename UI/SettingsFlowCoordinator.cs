using System;
using System.Linq;
using Camera2.Behaviours;
using Camera2.Managers;
using HMUI;
using JetBrains.Annotations;
using Zenject;

namespace Camera2.UI
{
    internal class SettingsFlowCoordinator : FlowCoordinator
    {
        internal static SettingsFlowCoordinator Instance { get; private set; }

        private MainFlowCoordinator _mainFlowCoordinator;
        private CameraSettingsViewController _cameraSettingsViewController;
        public CameraListViewController CameraListViewController { get; private set; }
        private CameraMovementSettingsViewController _cameraMovementSettingsViewController;
        private CameraPreviewViewController _cameraPreviewViewController;

        private Cam2 _lastSelected;
        private bool _isActive;

        [Inject]
        [UsedImplicitly]
        public void Construct(
            MainFlowCoordinator mainFlowCoordinator,
            CameraSettingsViewController cameraSettingsViewController,
            CameraListViewController cameraListViewController,
            CameraMovementSettingsViewController cameraMovementSettingsViewController,
            CameraPreviewViewController cameraPreviewViewController
        )
        {
            Instance = this;

            _mainFlowCoordinator = mainFlowCoordinator;
            _cameraSettingsViewController = cameraSettingsViewController;
            CameraListViewController = cameraListViewController;
            _cameraMovementSettingsViewController = cameraMovementSettingsViewController;
            _cameraPreviewViewController = cameraPreviewViewController;
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

            if (!_cameraSettingsViewController.SetCam(cam) && !reSelect)
            {
                return;
            }

            UpdateTitle(cam);
            _cameraMovementSettingsViewController.SetCam(cam);

            var cellIndex = Array.FindIndex(CameraListViewController.ListDataOrdered.ToArray(), x => x.Cam.Name == cam.Name);

            CameraListViewController.list.tableView.SelectCellWithIdx(cellIndex);
            CameraListViewController.list.tableView.ScrollToCellWithIdx(cellIndex, TableView.ScrollPositionType.Center, false);

            if (reSelect)
            {
                _cameraSettingsViewController.ReselectLastTab();
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

            if (firstActivation)
            {
                showBackButton = true;
                ProvideInitialViewControllers(_cameraSettingsViewController, CameraListViewController, _cameraMovementSettingsViewController, _cameraPreviewViewController);
            }
            else
            {
                if (CameraListViewController != null)
                {
                    CameraListViewController.Init();
                }
                
                if (_lastSelected != null)
                {
                    ShowSettingsForCam(_lastSelected);
                }
            }
        }


#if V1_29_1
        public
#else
        protected
#endif
            override void BackButtonWasPressed(ViewController topViewController)
        {
            _cameraSettingsViewController.SetCam(null);
            _cameraMovementSettingsViewController.SetCam(null);
            ScenesManager.LoadGameScene(forceReload: true);
            _mainFlowCoordinator.DismissFlowCoordinator(this);
            _isActive = false;
        }
    }
}