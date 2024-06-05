﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using Camera2.Behaviours;
using Camera2.Enums;
using Camera2.Managers;
using HMUI;
using JetBrains.Annotations;

namespace Camera2.UI
{
    class CamList : BSMLResourceViewController
    {
        private List<object> Cams => ListDataOrdered.ToList<object>();

        private readonly List<CamListCellWrapper> _listData = new List<CamListCellWrapper>();

        public override string ResourceName => "Camera2.UI.Views.camList.bsml";

        [UsedImplicitly] private string _cam2Version = $"Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} by Kinsi55 modified by UnskilledFreak";

        [UIComponent("deleteButton")] [UsedImplicitly]
        public NoTransitionsButton deleteButton;

        [UIComponent("camList")] [UsedImplicitly]
        public CustomCellListTableData list;

        public IEnumerable<CamListCellWrapper> ListDataOrdered => _listData.OrderByDescending(x => x.Cam.Settings.Layer);

        private void UpdateCamListUI()
        {
            //var x = Sprite.Create(cam.screenImage.material, new Rect(0, 0, cam.renderTexture.width, cam.renderTexture.width), new Vector2(0.5f, 0.5f));
            list.data = Cams;
            list.tableView.ReloadData();
            deleteButton.interactable = _listData.Count > 1;
        }

        [UIAction("#post-parse")]
        internal void Init()
        {
            _listData.Clear();

            _listData.AddRange(CamManager.Cams.Values.Select(x => new CamListCellWrapper(x)));
            UpdateCamListUI();
        }

        [UIAction("SelectCamera")]
        [UsedImplicitly]
        private void SelectCamera(TableView tableView, CamListCellWrapper row) => Coordinator.Instance.ShowSettingsForCam(row.Cam);

        private static Cam2 GetNewCam(string name)
        {
            var cam = CamManager.AddNewCamera(name);
            cam.Settings.SetViewRect(
                UnityEngine.Random.Range(0, 0.2f),
                UnityEngine.Random.Range(0, 0.2f),
                1f / 3,
                1f / 3
            );

            return cam;
        }

        private void AddCam(Cam2 cam)
        {
            cam.Settings.ApplyPositionAndRotation();
            cam.Settings.ApplyLayerBitmask();
            cam.UpdateRenderTextureAndView();

            _listData.Insert(0, new CamListCellWrapper(cam));
            UpdateCamListUI();
            Coordinator.Instance.ShowSettingsForCam(cam);
        }

        [UsedImplicitly]
        private void AddCamDefault() => AddCam(GetNewCam("Unnamed Camera"));

        [UsedImplicitly]
        private void AddCamSideView()
        {
            var cam = CamManager.AddNewCamera("Side View");

            cam.Settings.Type = CameraType.Positionable;
            cam.Settings.FOV = 75;
            cam.Settings.SetViewRect(0, 0, .195f, .395f);
            cam.Settings.TargetPos = new UnityEngine.Vector3(3, 1.2f, 0);
            cam.Settings.TargetRot = new UnityEngine.Vector3(0, -90f, 0);
            cam.Settings.VisibleObjects.Walls = WallVisibility.Hidden;
            cam.Settings.VisibleObjects.Debris = false;
            cam.Settings.VisibleObjects.UI = false;
            cam.Settings.VisibleObjects.Floor = false;
            cam.Settings.VisibleObjects.CutParticles = false;
            cam.Settings.FarZ = 10f;

            AddCam(cam);
        }
        
        [UsedImplicitly]
        private void AddCamBackView()
        {
            var cam = CamManager.AddNewCamera("Back View");

            cam.Settings.Type = CameraType.Positionable;
            cam.Settings.FOV = 42;
            cam.Settings.SetViewRect(0.8f, 0, .2f, .3f);
            cam.Settings.TargetPos = new UnityEngine.Vector3(0, 1.5f, -1.3f);
            cam.Settings.VisibleObjects.Walls = WallVisibility.Hidden;
            cam.Settings.VisibleObjects.Debris = false;
            cam.Settings.VisibleObjects.UI = false;
            cam.Settings.VisibleObjects.Floor = false;
            cam.Settings.VisibleObjects.CutParticles = false;
            cam.Settings.FarZ = 6f;
            cam.Settings.Orthographic = true;

            AddCam(cam);
        }

        [UsedImplicitly]
        private void AddCamThirdPerson()
        {
            var cam = GetNewCam("Static ThirdPerson");

            cam.Settings.Type = CameraType.Positionable;
            cam.Settings.FOV = 75;
            cam.Settings.TargetPos = new UnityEngine.Vector3(1.93f, 2.32f, -2.45f);
            cam.Settings.TargetRot = new UnityEngine.Vector3(16.48f, 335.78f, 0.81f);

            AddCam(cam);
        }

        [UsedImplicitly]
        private void AddCamAvatarFaceCam()
        {
            var cam = GetNewCam("FaceCam");

            cam.Settings.Type = CameraType.FirstPerson;
            cam.Settings.FOV = 75;
            cam.Settings.TargetPos = new UnityEngine.Vector3(0, -0.1f, 0.5f);
            cam.Settings.TargetRot = new UnityEngine.Vector3(0, 180f, 0);

            cam.Settings.SmoothFollow.FollowReplayPosition = false;
            cam.Settings.SmoothFollow.Limits.RotZ = "0:0";
            cam.Settings.SmoothFollow.Position = 8f;
            cam.Settings.SmoothFollow.Rotation = 3f;

            cam.Settings.VisibleObjects.Avatar = AvatarVisibility.ForceVisibleInFP;
            cam.Settings.VisibleObjects.UI = false;

            AddCam(cam);
        }

        [UsedImplicitly]
        private void AddCamFollowAvatarFaceCam()
        {
            var cam = GetNewCam("Follower Cam");

            cam.Settings.Type = CameraType.Follower;
            cam.Settings.FOV = 75;
            cam.Settings.TargetPos = new UnityEngine.Vector3(1.93f, 2.32f, -2.45f);
            cam.Settings.TargetRot = new UnityEngine.Vector3(16.48f, 335.78f, 0.81f);

            cam.Settings.SmoothFollow.FollowReplayPosition = false;
            cam.Settings.SmoothFollow.Position = 15f;
            cam.Settings.SmoothFollow.Rotation = 6f;
            cam.Settings.SmoothFollow.TargetParent = "NalulunaCatController/NalulunaCat(Clone)/Cat/Root";
            //cam.Settings.SmoothFollow.PivotingOffset = false;

            cam.Settings.VisibleObjects.Avatar = AvatarVisibility.Visible;
            cam.Settings.VisibleObjects.UI = false;

            AddCam(cam);
        }

        [UsedImplicitly]
        private void DeleteCam()
        {
            _listData.Remove(_listData.Find(x => x.Cam == SettingsView.CurrentCam));
            CamManager.DeleteCamera(SettingsView.CurrentCam);
            UpdateCamListUI();
            Coordinator.Instance.ShowSettingsForCam(ListDataOrdered.First().Cam);
        }

        private static void ChangeLayer(int diff)
        {
            SettingsView.CurrentCam.Settings.Layer += diff;
            Coordinator.Instance.CamList.UpdateCamListUI();
            Coordinator.Instance.ShowSettingsForCam(SettingsView.CurrentCam, true);
        }

        [UsedImplicitly]
        private void LayerIncrease() => ChangeLayer(1);
        
        [UsedImplicitly]
        private void LayerDecrease() => ChangeLayer(-1);

        [UsedImplicitly]
        private void UnlockCamPosTab() => Coordinator.Instance.SettingsView.viewRectTab.IsVisible = true;

        [UsedImplicitly]
        private void ShowGithub() => Process.Start("https://github.com/kinsi55/CS_BeatSaber_Camera2");
        
        [UsedImplicitly]
        private void ShowWiki() => Process.Start("https://github.com/kinsi55/CS_BeatSaber_Camera2/wiki");
    }
}