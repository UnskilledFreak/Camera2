using Camera2.Behaviours;
using Camera2.HarmonyPatches;
using Camera2.Managers;
using Camera2.SDK;
using Camera2.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Linq;
using Camera2.Enums;
using Camera2.JsonConverter;
using JetBrains.Annotations;
using UnityEngine;
using CameraType = Camera2.Enums.CameraType;

namespace Camera2.Configuration
{
    internal class CameraSettings
    {
        internal Cam2 Cam { get; private set; }

        internal OverrideToken OverrideToken;

        internal bool IsScreenLocked
        {
            get => ViewRect.Locked;
            set => ViewRect.Locked = value;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public CameraType Type
        {
            get => _type;
            set
            {
                _type = value;

                if (!IsLoaded)
                {
                    return;
                }

                Cam.ShowWorldCamIfNecessary();
                ApplyLayerBitmask();
                // Pos / Rot is applied differently depending on if it's a FP or TP cam
                ApplyPositionAndRotation();
            }
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public WorldCamVisibility WorldCamVisibility
        {
            get => _worldCamVisibility;
            set
            {
                _worldCamVisibility = value;
                if (IsLoaded)
                {
                    Cam.ShowWorldCamIfNecessary();
                }
            }
        }

        public float PreviewScreenSize
        {
            get => _previewScreenSize;
            set
            {
                _previewScreenSize = Mathf.Clamp(value, 0.3f, 3f);
                if (IsLoaded && Cam.WorldCam != null)
                {
                    Cam.WorldCam.SetPreviewPositionAndSize();
                }
            }
        }

        public bool WorldCamUnderScreen
        {
            get => _worldCamUnderScreen;
            set
            {
                _worldCamUnderScreen = value;
                if (IsLoaded && Cam.WorldCam != null)
                {
                    Cam.WorldCam.SetPreviewPositionAndSize();
                }
            }
        }

        public float FOV
        {
            get => OverrideToken?.FOV ?? _fov;
            set
            {
                _fov = Cam.Camera.fieldOfView = value;
                Cam.Camera.orthographicSize = _fov * 0.0333f;
            }
        }

        public int Layer
        {
            get => (int)Cam.Camera.depth;
            set
            {
                Cam.Camera.depth = value;
                if (IsLoaded)
                {
                    CamManager.ApplyCameraValues(viewLayer: true);
                }
            }
        }

        public int AntiAliasing
        {
            get => _antiAliasing;
            set
            {
                _antiAliasing = Mathf.Clamp(value, 1, 8);
                if (IsLoaded)
                {
                    Cam.UpdateRenderTextureAndView();
                }
            }
        }

        public float RenderScale
        {
            get => _renderScale;
            set
            {
                _renderScale = Mathf.Clamp(value, 0.2f, 3f);
                if (IsLoaded)
                {
                    Cam.UpdateRenderTextureAndView();
                }
            }
        }

        public bool Orthographic
        {
            get => Cam.Camera.orthographic;
            set => Cam.Camera.orthographic = value;
        }

        public float FarZ
        {
            get => Cam.Camera.farClipPlane;
            set
            {
                /* TODO: Remove this at some point 😀
                 * Ingame farZ changed to 5k from 1k at some point without me noticing
                 */
                if (value > 5000f)
                {
                    value = 5000f;
                }

                Cam.Camera.farClipPlane = Mathf.Max(value);
            }
        }

        public float NearZ
        {
            get => Cam.Camera.nearClipPlane;
            set => Cam.Camera.nearClipPlane = Mathf.Max(0.00f, value);
        }

        [JsonIgnore] public GameObjects VisibleObjects => OverrideToken?.VisibleObjects ?? _visibleObjects;

        [JsonIgnore] internal ScreenRect ViewRect { get; private set; }

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 TargetPos
        {
            get => OverrideToken?.Position ?? _targetPos;
            set => _targetPos = value;
        }

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 TargetRot
        {
            get => OverrideToken?.Rotation ?? _targetRot;
            set => _targetRot = value;
        }

        [JsonProperty("visibleObjects")] private GameObjects _visibleObjects;

        [JsonConverter(typeof(ScreenRectConverter)), JsonProperty("viewRect")]
        private ScreenRect ViewRectCfg
        {
            get => ViewRect;
            set
            {
                value.Width = Math.Min(2, Math.Abs(value.Width));
                value.Height = Math.Min(2, Math.Abs(value.Height));

                var x = value.Width * .75f;
                var y = value.Height * .75f;

                value.X = Mathf.Clamp(value.X, -x, Math.Max(0.75f, 1 - x));
                value.Y = Mathf.Clamp(value.Y, -y, Math.Max(0.75f, 1 - y));

                ViewRect = value;
            }
        }

        [JsonIgnore]
        [CanBeNull]
        internal Transform Parent
        {
            get
            {
                if (_parent != null)
                {
                    return _parent;
                }

                var tmp = GameObject.Find(SmoothFollow.TargetParent)?.transform;
                /*
                if (tmp == null)
                {
                    Cam.LogError("target not found: " + SmoothFollow.TargetParent);
                }
                else
                {
                    Cam.LogDebug("target found: " + SmoothFollow.TargetParent);
                }
                */
                    
                _parent = tmp;

                return _parent;
            }
        }
        
        [JsonIgnore]
        private Transform _parent;

        private bool IsLoaded { get; set; }
        private CameraType _type = CameraType.FirstPerson;
        private WorldCamVisibility _worldCamVisibility = WorldCamVisibility.HiddenWhilePlaying;
        private float _previewScreenSize = 0.3f;
        private bool _worldCamUnderScreen = true;
        private float _fov;
        private int _antiAliasing = 2;
        private float _renderScale = 1.3f;
        private Vector3 _targetPos = Vector3.zero;
        private Vector3 _targetRot = Vector3.zero;

        public SettingsMultiplayer Multiplayer { get; private set; }
        public SettingsSmoothFollow SmoothFollow { get; private set; }
        public SettingsModmapExtensions ModMapExtensions { get; private set; }
        public SettingsFollow360 Follow360 { get; private set; }
        public SettingsVmcAvatar VmcProtocol { get; private set; }
        public SettingsFPSLimiter FPSLimiter { get; private set; }
        public SettingsPostProcessing PostProcessing { get; private set; }
        public SettingsMovementScript MovementScript { get; private set; } = new SettingsMovementScript();

        public CameraSettings(Cam2 cam)
        {
            Cam = cam;

            _visibleObjects = new GameObjects(this);

            Multiplayer = CameraSubSettings.GetFor<SettingsMultiplayer>(this);
            FPSLimiter = CameraSubSettings.GetFor<SettingsFPSLimiter>(this);
            SmoothFollow = CameraSubSettings.GetFor<SettingsSmoothFollow>(this);
            ModMapExtensions = CameraSubSettings.GetFor<SettingsModmapExtensions>(this);
            Follow360 = CameraSubSettings.GetFor<SettingsFollow360>(this);
            VmcProtocol = CameraSubSettings.GetFor<SettingsVmcAvatar>(this);
            PostProcessing = CameraSubSettings.GetFor<SettingsPostProcessing>(this);
        }

        public void ParentChange()
        {
            _parent = null;
        }

        public bool IsPositionalCam() => Type == CameraType.Positionable || Type == CameraType.Follower;

        public void Load(bool loadConfig = true)
        {
            _parent = null;
            IsLoaded = false;
            // Set default value in case they're not loaded from JSON
            FOV = 90f;

            if (System.IO.File.Exists(Cam.ConfigPath))
            {
                if (loadConfig)
                {
                    JsonConvert.PopulateObject(System.IO.File.ReadAllText(Cam.ConfigPath), this, JsonHelpers.LeanDeserializeSettings);
                }
            }
            else
            {
                Layer = CamManager.Cams.Count == 0 
                    ? 1 
                    : CamManager.Cams.Max(x => x.Value.Settings.Layer) + 1;
            }

            // We always save after loading, even if it's a fresh load. This will make sure to migrate configs after updates.
            Save();

            ViewRect ??= new ScreenRect(0, 0, 1, 1, false);
            
            ApplyPositionAndRotation();
            ApplyLayerBitmask();
            Cam.UpdateRenderTextureAndView();
            Cam.ShowWorldCamIfNecessary();
            IsLoaded = true;
        }

        public void Save()
        {
            if (Cam == null)
            {
                return;
            }

            var x = OverrideToken;
            OverrideToken = null;
            try
            {
                System.IO.File.WriteAllText(Cam.ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Cam.LogError($"Failed to save Config for Camera {Cam.Name}:");
                Cam.LogError(ex);
            }

            OverrideToken = x;
        }

        public void Reload()
        {
            Load();
            foreach (var x in Cam.Middlewares)
            {
                x.CamConfigReloaded();
            }
        }

        public void ApplyPositionAndRotation()
        {
            Cam.Transformer.Position = TargetPos;
            Cam.Transformer.RotationEuler = TargetRot;

            // Force pivoting offset for 360 Levels - Non-Pivoting offset on 360 levels just looks outright trash
            Cam.Transformer.ApplyAsAbsolute = !IsPositionalCam() && !SmoothFollow.PivotingOffset && !HookLeveldata.Is360Level;
        }

        public void ApplyLayerBitmask()
        {
            var maskBuilder = VisibleObjects.EverythingElse ? (VisibilityMasks)CamManager.ClearedBaseCullingMask : 0;

            if (VisibleObjects.Walls == WallVisibility.Visible || (ModMapExtensions.AutoOpaqueWalls && HookLeveldata.IsWallMap))
            {
                maskBuilder |= VisibilityMasks.Walls | VisibilityMasks.WallTextures;
            }
            else if (VisibleObjects.Walls == WallVisibility.Transparent)
            {
                maskBuilder |= VisibilityMasks.Walls;
            }

            if (VisibleObjects.Notes != NoteVisibility.Hidden)
            {
                if (VisibleObjects.Notes == NoteVisibility.ForceCustomNotes && CustomNotesUtil.HasHMDOnlyEnabled())
                {
                    maskBuilder |= VisibilityMasks.CustomNotes;
                }
                else
                {
                    maskBuilder |= VisibilityMasks.Notes;
                }
            }

            if (VisibleObjects.Avatar != AvatarVisibility.Hidden)
            {
                maskBuilder |= VisibilityMasks.Avatar;

                maskBuilder |= (Type == CameraType.FirstPerson && VisibleObjects.Avatar != AvatarVisibility.ForceVisibleInFP) ? VisibilityMasks.FirstPersonAvatar : VisibilityMasks.ThirdPersonAvatar;
            }

            if (VisibleObjects.Floor)
            {
                maskBuilder |= VisibilityMasks.Floor | VisibilityMasks.PlayerPlatform;
            }

            if (VisibleObjects.Debris)
            {
                maskBuilder |= VisibilityMasks.Debris;
            }

            if (VisibleObjects.CutParticles)
            {
                maskBuilder |= VisibilityMasks.CutParticles;
            }

            if (VisibleObjects.Sabers)
            {
                maskBuilder |= VisibilityMasks.Sabers;
            }

            if (VisibleObjects.UI && (!ModMapExtensions.AutoHideHUD || !HookLeveldata.IsWallMap))
            {
                maskBuilder |= VisibilityMasks.UI;
            }

            if (Cam.Camera.cullingMask != (int)maskBuilder)
            {
                Cam.Camera.cullingMask = (int)maskBuilder;
            }
        }

        public void UnOverriden(Action accessor)
        {
            var x = OverrideToken;
            OverrideToken = null;
            try
            {
                accessor();
            }
            finally
            {
                OverrideToken = x;
            }
        }

        public void SetViewRect(float? x, float? y, float? width, float? height)
        {
            ViewRect.X = x ?? ViewRect.X;
            ViewRect.Y = y ?? ViewRect.Y;
            ViewRect.Width = width ?? ViewRect.Width;
            ViewRect.Height = height ?? ViewRect.Height;

            ViewRectCfg = ViewRect;

            if (IsLoaded)
            {
                Cam.UpdateRenderTextureAndView();
            }
        }

        // some Newtonsoft magic
        [UsedImplicitly]
        public bool ShouldSerializeWorldCamVisibility() => IsPositionalCam();

        [UsedImplicitly]
        public bool ShouldSerializepreviewScreenSize() => IsPositionalCam();

        [UsedImplicitly]
        public bool ShouldSerializeWorldCamUnderScreen() => IsPositionalCam();

        [UsedImplicitly]
        public bool ShouldSerializeFollow360() => IsPositionalCam();

        [UsedImplicitly]
        public bool ShouldSerializeSmoothFollow() => Type != CameraType.Positionable;

        [UsedImplicitly]
        public bool ShouldSerializeVmcProtocol() => IsPositionalCam();

        [UsedImplicitly]
        public bool ShouldSerializeMovementScript() => Type != CameraType.FirstPerson;
    }
}