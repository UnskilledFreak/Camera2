using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using Camera2.Behaviours;
using Camera2.Enums;
using Camera2.Managers;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.UI
{
    internal class SettingsView : BSMLResourceViewController
    {
        public override string ResourceName => "Camera2.UI.Views.camSettings.bsml";
        internal static Cam2 CurrentCam { get; private set; }

        [UsedImplicitly]
        //private static readonly List<object> Types = new List<object> { CameraType.FirstPerson, CameraType.Positionable };
        public static readonly List<object> Types = Enum.GetValues(typeof(CameraType)).Cast<object>().ToList();

        [UsedImplicitly] private static readonly List<object> AntiAliasingLevels = new List<object> { 1, 2, 4, 8 };
        [UsedImplicitly] private static readonly List<object> WorldCamVisibilities = Enum.GetValues(typeof(WorldCamVisibility)).Cast<object>().ToList();
        [UsedImplicitly] private static readonly List<object> VisibilitiesWalls = Enum.GetValues(typeof(WallVisibility)).Cast<object>().ToList();
        [UsedImplicitly] private static readonly List<object> VisibilitiesNotes = Enum.GetValues(typeof(NoteVisibility)).Cast<object>().ToList();
        [UsedImplicitly] private static readonly List<object> VisibilitiesAvatar = Enum.GetValues(typeof(AvatarVisibility)).Cast<object>().ToList();

        [UsedImplicitly] private static string[] props;

        [UsedImplicitly] private SceneToggle[] _scenes;

        #region UI Components

        [UIComponent("zOffsetSlider")] [UsedImplicitly]
        public SliderSetting zOffsetSlider;

        [UIComponent("xRotationSlider")] [UsedImplicitly]
        public SliderSetting xRotationSlider;

        [UIComponent("pivotingOffsetToggle")] [UsedImplicitly]
        public ToggleSetting pivotingOffsetToggle;

        [UIComponent("previewSizeSlider")] [UsedImplicitly]
        public SliderSetting previewSizeSlider;

        [UIComponent("modMapExtMoveWithMapCheckbox")] [UsedImplicitly]
        public ToggleSetting modMapExtMoveWithMapSlider;

        [UIComponent("worldCamVisibilityInput")] [UsedImplicitly]
        public LayoutElement worldCamVisibilityObj;

        [UIComponent("smoothFollowTab")] [UsedImplicitly]
        public Tab smoothFollowTab;

        [UIComponent("follow360Tab")] [UsedImplicitly]
        public Tab follow360Tab;

        [UIComponent("attachingTab")] [UsedImplicitly]
        public Tab attachingTab;

        [UIComponent("viewRectTab")] [UsedImplicitly]
        public Tab viewRectTab;

        [UIComponent("tabSelector")] [UsedImplicitly]
        public TabSelector tabSelector;

        #endregion

        #region UI Getter and Setter

        // public makes it impossible for BSML to access?

        [UsedImplicitly]
        internal string CamName
        {
            get => CurrentCam.Name;
            set
            {
                if (CamManager.RenameCamera(CurrentCam, value))
                {
                    Coordinator.Instance.CamList.list.tableView.ReloadData();
                }

                NotifyPropertyChanged();
            }
        }

        [UsedImplicitly]
        internal CameraType Type
        {
            get => CurrentCam.Settings.Type;
            set
            {
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (value)
                {
                    // When switching to FP reset Rot / Pos so that the previous TP values arent used as the FP offset
                    case CameraType.FirstPerson:
                        CurrentCam.Settings.TargetPos = new Vector3(0, 0, 0);
                        NotifyPropertyChanged("zOffset");
                        break;
                    case CameraType.Positionable:
                    case CameraType.Follower:
                        CurrentCam.Settings.TargetPos = new Vector3(0, 1.5f, 1f);
                        break;
                }

                CurrentCam.Settings.TargetRot = Vector3.zero;
                CurrentCam.Settings.Type = value;
                ToggleSettingVisibility();
            }
        }

        [UsedImplicitly]
        internal WorldCamVisibility WorldCamVisibility
        {
            get => CurrentCam.Settings.WorldCamVisibility;
            set => CurrentCam.Settings.WorldCamVisibility = value;
        }

        [UsedImplicitly]
        internal float FOV
        {
            get => CurrentCam.Settings.FOV;
            set => CurrentCam.Settings.FOV = value;
        }

        [UsedImplicitly]
        internal int FpsLimit
        {
            get => CurrentCam.Settings.FPSLimiter.FPSLimit;
            set => CurrentCam.Settings.FPSLimiter.FPSLimit = value;
        }

        [UsedImplicitly]
        internal float RenderScale
        {
            get => CurrentCam.Settings.RenderScale;
            set => CurrentCam.Settings.RenderScale = value;
        }

        [UsedImplicitly]
        internal int AntiAliasing
        {
            get => CurrentCam.Settings.AntiAliasing;
            set => CurrentCam.Settings.AntiAliasing = value;
        }

        [UsedImplicitly]
        internal float PreviewSize
        {
            get => CurrentCam.Settings.PreviewScreenSize;
            set => CurrentCam.Settings.PreviewScreenSize = value;
        }

        [UsedImplicitly]
        internal float ZOffset
        {
            get
            {
                var ret = 0f;
                CurrentCam.Settings.UnOverriden(delegate
                {
                    ret = CurrentCam.Settings.TargetPos.z;
                });
                return ret;
            }
            set
            {
                CurrentCam.Settings.UnOverriden(delegate
                {
                    var x = CurrentCam.Settings.TargetPos;
                    x.z = value;

                    CurrentCam.Settings.TargetPos = x;
                });
                CurrentCam.Settings.ApplyPositionAndRotation();
            }
        }

        [UsedImplicitly]
        internal WallVisibility VisibilityWalls
        {
            get => CurrentCam.Settings.VisibleObjects.Walls;
            set => CurrentCam.Settings.VisibleObjects.Walls = value;
        }

        [UsedImplicitly]
        internal NoteVisibility VisibilityNotes
        {
            get => CurrentCam.Settings.VisibleObjects.Notes;
            set => CurrentCam.Settings.VisibleObjects.Notes = value;
        }

        [UsedImplicitly]
        internal bool VisibilityDebris
        {
            get => CurrentCam.Settings.VisibleObjects.Debris;
            set => CurrentCam.Settings.VisibleObjects.Debris = value;
        }

        [UsedImplicitly]
        internal bool VisibilityUI
        {
            get => CurrentCam.Settings.VisibleObjects.UI;
            set => CurrentCam.Settings.VisibleObjects.UI = value;
        }

        [UsedImplicitly]
        internal AvatarVisibility VisibilityAvatar
        {
            get => CurrentCam.Settings.VisibleObjects.Avatar;
            set => CurrentCam.Settings.VisibleObjects.Avatar = value;
        }

        [UsedImplicitly]
        internal bool VisibilityFloor
        {
            get => CurrentCam.Settings.VisibleObjects.Floor;
            set => CurrentCam.Settings.VisibleObjects.Floor = value;
        }

        [UsedImplicitly]
        internal bool VisibilityCutParticles
        {
            get => CurrentCam.Settings.VisibleObjects.CutParticles;
            set => CurrentCam.Settings.VisibleObjects.CutParticles = value;
        }

        [UsedImplicitly]
        internal bool VisibilitySabers
        {
            get => CurrentCam.Settings.VisibleObjects.Sabers;
            set => CurrentCam.Settings.VisibleObjects.Sabers = value;
        }

        [UsedImplicitly]
        internal bool VisibilityEverythingElse
        {
            get => CurrentCam.Settings.VisibleObjects.EverythingElse;
            set => CurrentCam.Settings.VisibleObjects.EverythingElse = value;
        }

        [UsedImplicitly]
        internal bool MultiplayerFollowSpectatorPlatform
        {
            get => CurrentCam.Settings.Multiplayer.FollowSpectatorPlatform;
            set => CurrentCam.Settings.Multiplayer.FollowSpectatorPlatform = value;
        }

        [UsedImplicitly]
        internal bool SmoothFollowForceUpright
        {
            get => CurrentCam.Settings.SmoothFollow.Limits.RotZ == "0:0";
            set
            {
                if (value)
                {
                    CurrentCam.Settings.SmoothFollow.Limits.RotZ = "0:0";
                }
                else
                {
                    CurrentCam.Settings.SmoothFollow.Limits.RotZMin = float.NegativeInfinity;
                    CurrentCam.Settings.SmoothFollow.Limits.RotZMax = float.PositiveInfinity;
                }
            }
        }

        [UsedImplicitly]
        internal bool SmoothFollowFollowReplayPosition
        {
            get => CurrentCam.Settings.SmoothFollow.FollowReplayPosition;
            set => CurrentCam.Settings.SmoothFollow.FollowReplayPosition = value;
        }

        [UsedImplicitly]
        internal float SmoothFollowPosition
        {
            get => CurrentCam.Settings.SmoothFollow.Position;
            set => CurrentCam.Settings.SmoothFollow.Position = value;
        }

        [UsedImplicitly]
        internal float SmoothFollowRotation
        {
            get => CurrentCam.Settings.SmoothFollow.Rotation;
            set => CurrentCam.Settings.SmoothFollow.Rotation = value;
        }

        [UsedImplicitly]
        internal bool ModMapExtMoveWithMap
        {
            get => CurrentCam.Settings.ModMapExtensions.MoveWithMap;
            set => CurrentCam.Settings.ModMapExtensions.MoveWithMap = value;
        }

        [UsedImplicitly]
        internal bool ModMapExtAutoOpaqueWalls
        {
            get => CurrentCam.Settings.ModMapExtensions.AutoOpaqueWalls;
            set => CurrentCam.Settings.ModMapExtensions.AutoOpaqueWalls = value;
        }

        [UsedImplicitly]
        internal bool ModMapExtAutoHideHUD
        {
            get => CurrentCam.Settings.ModMapExtensions.AutoHideHUD;
            set => CurrentCam.Settings.ModMapExtensions.AutoHideHUD = value;
        }

        [UsedImplicitly]
        internal bool Follow360MoveWithMap
        {
            get => CurrentCam.Settings.Follow360.Enabled;
            set => CurrentCam.Settings.Follow360.Enabled = value;
        }

        [UsedImplicitly]
        internal float Follow360Smoothing
        {
            get => CurrentCam.Settings.Follow360.Smoothing;
            set => CurrentCam.Settings.Follow360.Smoothing = value;
        }

        [UsedImplicitly]
        internal float PostprocessingTransparencyThreshold
        {
            get => CurrentCam.Settings.PostProcessing.TransparencyThreshold;
            set => CurrentCam.Settings.PostProcessing.TransparencyThreshold = value;
        }

        [UsedImplicitly]
        internal bool PostprocessingForceDepthTexture
        {
            get => CurrentCam.Settings.PostProcessing.ForceDepthTexture;
            set => CurrentCam.Settings.PostProcessing.ForceDepthTexture = value;
        }

        [UsedImplicitly]
        internal float ViewRectX
        {
            get => CurrentCam.Settings.ViewRect.X;
            set => CurrentCam.Settings.SetViewRect(value, null, null, null);
        }

        [UsedImplicitly]
        internal float ViewRectY
        {
            get => CurrentCam.Settings.ViewRect.Y;
            set => CurrentCam.Settings.SetViewRect(null, value, null, null);
        }

        [UsedImplicitly]
        internal float ViewRectWidth
        {
            get => CurrentCam.Settings.ViewRect.Width;
            set => CurrentCam.Settings.SetViewRect(null, null, value, null);
        }

        [UsedImplicitly]
        internal float ViewRectHeight
        {
            get => CurrentCam.Settings.ViewRect.Height;
            set => CurrentCam.Settings.SetViewRect(null, null, null, value);
        }

        [UsedImplicitly]
        internal bool ViewRectIsLocked
        {
            get => CurrentCam.Settings.IsScreenLocked;
            set => CurrentCam.Settings.IsScreenLocked = value;
        }

        [UsedImplicitly]
        internal float XRotation
        {
            get => GetUnOverridenRotation().x;
            set => SetAndUpdateUnOverridenRotation(value);
        }

        [UsedImplicitly]
        internal bool MiscPivotingOffset
        {
            get => CurrentCam.Settings.SmoothFollow.PivotingOffset;
            set => CurrentCam.Settings.SmoothFollow.PivotingOffset = value;
        }

        [UsedImplicitly]
        internal bool MiscOrthographic
        {
            get => CurrentCam.Settings.Orthographic;
            set => CurrentCam.Settings.Orthographic = value;
        }

        [UsedImplicitly]
        internal bool MiscMovementScriptEnableInMenu
        {
            get => CurrentCam.Settings.MovementScript.EnableInMenu;
            set => CurrentCam.Settings.MovementScript.EnableInMenu = value;
        }

        [UsedImplicitly]
        internal string TargetParent
        {
            get => CurrentCam.Settings.SmoothFollow.TargetParent;
            set
            {
                CurrentCam.Settings.SmoothFollow.TargetParent = value;
                CurrentCam.Settings.ParentChange();
                NotifyPropertyChanged();
            }
        }

        [UsedImplicitly]
        internal float TargetPosX
        {
            get => GetUnOverridenPosition().x;
            set
            {
                SetAndUpdateUnOverridenPosition(value);
                NotifyPropertyChanged();
            }
        }

        [UsedImplicitly]
        internal float TargetPosY
        {
            get => GetUnOverridenPosition().y;
            set
            {
                SetAndUpdateUnOverridenPosition(y: value);
                NotifyPropertyChanged();
            }
        }

        [UsedImplicitly]
        internal float TargetPosZ
        {
            get => GetUnOverridenPosition().z;
            set
            {
                SetAndUpdateUnOverridenPosition(z: value);
                NotifyPropertyChanged();
            }
        }

        [UsedImplicitly]
        internal float TargetRotX
        {
            get => GetUnOverridenRotation().x;
            set
            {
                SetAndUpdateUnOverridenRotation(value);
                NotifyPropertyChanged();
            }
        }

        [UsedImplicitly]
        internal float TargetRotY
        {
            get => GetUnOverridenRotation().y;
            set
            {
                SetAndUpdateUnOverridenRotation(y: value);
                NotifyPropertyChanged();
            }
        }

        [UsedImplicitly]
        internal float TargetRotZ
        {
            get => GetUnOverridenRotation().z;
            set
            {
                SetAndUpdateUnOverridenRotation(z: value);
                NotifyPropertyChanged();
            }
        }

        [UsedImplicitly]
        internal float VisibilityNearZ
        {
            get => CurrentCam.Settings.NearZ;
            set
            {
                CurrentCam.Settings.NearZ = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region UI Buttons

        [UIAction("SetRenderDistanceNear")]
        [UsedImplicitly]
        public void SetRenderDistanceNear() => CurrentCam.Settings.FarZ = 6f;

        [UIAction("SetRenderDistanceShort")]
        [UsedImplicitly]
        public void SetRenderDistanceShort() => CurrentCam.Settings.FarZ = 10f;

        [UIAction("SetRenderDistanceUnlimited")]
        [UsedImplicitly]
        public void SetRenderDistanceUnlimited() => CurrentCam.Settings.FarZ = 5000f;

        #endregion

        public void Awake()
        {
            // Don't really care which cam it is, this is just for BSML to init
            CurrentCam = CamManager.Cams.Values.First();

            props ??= typeof(SettingsView).GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .Select(x => x.Name)
                .ToArray();

            _scenes ??= Enum.GetValues(typeof(SceneTypes))
                .Cast<SceneTypes>()
                .Select(x => new SceneToggle { Type = x, Host = this })
                .ToArray();
        }

        internal bool SetCam(Cam2 newCam)
        {
            if (CurrentCam == newCam)
            {
                return false;
            }

            SaveSettings();
            CurrentCam = newCam;

            ScenesManager.LoadGameScene(forceReload: true);

            //if(Coordinator.instance.previewView?.dd != null) {
            //	Coordinator.instance.previewView.dd.material = cam?.worldCam.viewMaterial;
            //	Coordinator.instance.previewView.dd.SetMaterialDirty();
            //}

            CamManager.ApplyCameraValues(worldCam: true, viewLayer: true);

            if (CurrentCam == null)
            {
                return true;
            }

            CurrentCam.gameObject.SetActive(true);

            ToggleSettingVisibility();

            foreach (var prop in props)
            {
                NotifyPropertyChanged(prop);
            }

            foreach (var x in _scenes)
            {
                x.NotifyPropertyChanged("Val");
            }

            return true;
        }

        private static void SaveSettings()
        {
            if (CurrentCam != null)
            {
                CurrentCam.Settings.Save();
            }

            ScenesManager.Settings.Save();
        }

        [UIAction("#post-parse")]
        [UsedImplicitly]
        public void Parsed()
        {
            CurrentCam = null;

            // Since BSML doesn't make all the inputs have the same width I need to do it myself for my own sanity
            foreach (var x in GetComponentsInChildren<StringSetting>(true))
            {
                var picker = x.transform.Find("ValuePicker/DecButton");
                picker.gameObject.SetActive(false);
                picker.parent.localScale = new Vector3(1.06f, 1f, 1f);
            }

            foreach (var x in GetComponentsInChildren<DropDownListSetting>(true))
            {
                x.transform.localScale = new Vector3(1.09f, 1f, 1f);
            }

            viewRectTab.IsVisible = false;

            Coordinator.Instance.ShowSettingsForCam(CamManager.Cams.Values.First());
        }

        private void ToggleSettingVisibility()
        {
            if (zOffsetSlider == null)
            {
                return;
            }

            zOffsetSlider.gameObject.SetActive(Type == CameraType.FirstPerson);
            xRotationSlider.gameObject.SetActive(Type == CameraType.FirstPerson);
            pivotingOffsetToggle.gameObject.SetActive(Type == CameraType.FirstPerson);
            previewSizeSlider.gameObject.SetActive(CurrentCam.Settings.IsPositionalCam());
            modMapExtMoveWithMapSlider.gameObject.SetActive(CurrentCam.Settings.IsPositionalCam());
            worldCamVisibilityObj.gameObject.SetActive(CurrentCam.Settings.IsPositionalCam());
            smoothFollowTab.IsVisible = Type == CameraType.FirstPerson;
            follow360Tab.IsVisible = CurrentCam.Settings.IsPositionalCam();
            attachingTab.IsVisible = Type == CameraType.Attached || Type == CameraType.Follower;

            // Apparently this is the best possible way to programmatically switch the selected tab
            tabSelector.textSegmentedControl.SelectCellWithNumber(0);
            AccessTools.Method(typeof(TabSelector), "TabSelected").Invoke(tabSelector, new object[] { tabSelector.textSegmentedControl, 0 });
        }

        private static Vector3 GetUnOverridenPosition()
        {
            var ret = Vector3.zero;
            CurrentCam.Settings.UnOverriden(delegate
            {
                ret = CurrentCam.Settings.TargetPos;
            });
            return ret;
        }

        private static Vector3 GetUnOverridenRotation()
        {
            var ret = Vector3.zero;
            CurrentCam.Settings.UnOverriden(delegate
            {
                ret = CurrentCam.Settings.TargetRot;
            });
            return ret;
        }

        private static void SetAndUpdateUnOverridenPosition(float? x = null, float? y = null, float? z = null)
        {
            CurrentCam.Settings.TargetPos = UpdateVector(GetUnOverridenPosition(), x, y, z);
            ;
            CurrentCam.Settings.ApplyPositionAndRotation();
        }

        private static void SetAndUpdateUnOverridenRotation(float? x = null, float? y = null, float? z = null)
        {
            CurrentCam.Settings.TargetRot = UpdateVector(GetUnOverridenRotation(), x, y, z);
            ;
            CurrentCam.Settings.ApplyPositionAndRotation();
        }

        private static Vector3 UpdateVector(Vector3 vector, float? x, float? y, float? z)
        {
            if (x.HasValue)
            {
                vector.x = x.Value;
            }

            if (y.HasValue)
            {
                vector.y = y.Value;
            }

            if (z.HasValue)
            {
                vector.z = z.Value;
            }

            return vector;
        }
    }
}