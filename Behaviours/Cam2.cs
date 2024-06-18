using Camera2.Configuration;
using Camera2.HarmonyPatches;
using Camera2.Interfaces;
using Camera2.Middlewares;
using Camera2.UI;
using Camera2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Camera2.Enums;
using Camera2.Managers;
using UnityEngine;

namespace Camera2.Behaviours
{
    internal class Cam2 : MonoBehaviour
    {
        private static readonly HashSet<string> CameraBehavioursToDestroy = new HashSet<string>
        {
            "AudioListener", 
            "LIV", 
            "MainCamera", 
            "MeshCollider"
        };

        internal string Name { get; private set; }
        internal Camera Camera { get; private set; }
        internal CameraSettings Settings { get; private set; }
        internal RenderTexture RenderTexture { get; private set; }
        internal CameraDesktopView PreviewImage { get; private set; }
        internal PositionableCam WorldCam { get; private set; }
        internal IMHandler[] Middlewares { get; private set; }
        internal float TimeSinceLastRender { get; private set; }
        internal bool Destroying { get; private set; }

        internal Transformer Transformer;
        internal TransformChain TransformChain;
        private ParentShield _shield;

        internal string ConfigPath => ConfigUtil.GetCameraPath(Name);

        internal bool IsCurrentlySelectedInSettings => SettingsCoordinator.Instance && SettingsCoordinator.Instance.CamSettings.isActiveAndEnabled && CamSettings.CurrentCam == this;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void LogInfo(string message) => Plugin.Log.Info($"Cam '{Name}': {message}");
        public void LogDebug(string message) => Plugin.Log.Debug($"Cam '{Name}': {message}");
        public void LogError(string message) => Plugin.Log.Error($"Cam '{Name}': {message}");

        public void LogError(Exception exception)
        {
            Plugin.Log.Error($"Cam '{Name}': Exception!");
            Plugin.Log.Error(exception);
        }

        public bool Rename(string newName)
        {
            newName = string.Concat(newName.Split(Path.GetInvalidFileNameChars())).Trim();
            if (newName.Length == 0)
            {
                return false;
            }

            if (newName == Name)
            {
                return true;
            }

            foreach (var scene in ScenesManager.Settings.Scenes.Values.Where(scene => scene.Contains(Name)))
            {
                scene.Remove(Name);
                scene.Add(newName);
            }

            Settings.Save();
            File.Move(ConfigPath, ConfigUtil.GetCameraPath(newName));
            Init(newName, rename: true);
            ScenesManager.Settings.Save();

            return true;
        }

        public void SetOrigin(Transform parent, bool startFromParentTransform = true, bool unparentOnDisable = true)
        {
            if (transform.parent == parent)
            {
                return;
            }

            if (parent == null)
            {
                transform.parent = null;

                DontDestroyOnLoad(gameObject);
            }
            else
            {
                if (_shield == null)
                {
                    _shield = new GameObject($"Cam2_{Name}_Parenter").AddComponent<ParentShield>();
                }

                _shield.Init(this, parent, !startFromParentTransform);

                transform.SetParent(_shield.transform, !startFromParentTransform);
            }

            Settings.ApplyPositionAndRotation();
        }

        internal void UpdateRenderTextureAndView()
        {
            var round = (int)Math.Round(Settings.ViewRect.Width * Screen.width * Settings.RenderScale);
            var height = (int)Math.Round(Settings.ViewRect.Height * Screen.height * Settings.RenderScale);

            var sizeChanged = RenderTexture == null || RenderTexture.width != round || RenderTexture.height != height || RenderTexture.antiAliasing != Settings.AntiAliasing;

            if (sizeChanged)
            {
                if (RenderTexture != null)
                {
                    RenderTexture.Release();
                }

                RenderTexture = new RenderTexture(round, height, 24)
                {
                    //, RenderTextureFormat.ARGB32
                    useMipMap = false, antiAliasing = Settings.AntiAliasing, anisoLevel = 1, useDynamicScale = false
                };

                Camera.aspect = (float)round / height;
                Camera.targetTexture = RenderTexture;

                if (WorldCam != null)
                {
                    WorldCam.SetSource(this);
                }

                PrepareMiddleWareRender(true);
            }

            if (PreviewImage != null && (sizeChanged || PreviewImage.Rect.anchorMin != Settings.ViewRect.MinAnchor()))
            {
                PreviewImage.SetSource(this);
            }
        }

        internal void ShowWorldCamIfNecessary()
        {
            if (WorldCam == null)
            {
                return;
            }

            var doShowCam = Settings.IsPositionalCam()
                            && Settings.WorldCamVisibility != WorldCamVisibility.Hidden
                            && (
                                Settings.WorldCamVisibility != WorldCamVisibility.HiddenWhilePlaying
                                || !SceneUtil.IsSongPlaying
                            );

            WorldCam.gameObject.SetActive(doShowCam || (CamSettings.CurrentCam == this && Settings.IsPositionalCam()));
        }

        internal void UpdateDepthTextureActive()
        {
            if (Camera != null)
            {
                Camera.depthTextureMode = InitOnMainAvailable.useDepthTexture || Settings?.PostProcessing.ForceDepthTexture == true ? DepthTextureMode.Depth : DepthTextureMode.None;
            }
        }

        public void Init(string camName, CameraDesktopView cameraDesktopView = null, bool loadConfig = false, bool rename = false)
        {
            if (Name != null)
            {
                if (!rename)
                {
                    return;
                }

                Name = camName;
                if (loadConfig)
                {
                    Settings.Load();
                }

                return;
            }

            Name = camName;
            PreviewImage = cameraDesktopView;

            var camClone = Instantiate(SceneUtil.GetMainCameraButReally(), Vector3.zero, Quaternion.identity, transform);
            camClone.name = "Cam";

            Camera = camClone.GetComponent<Camera>();
            Camera.farClipPlane = 5000f;
            Camera.enabled = false;
            Camera.tag = "Untagged";
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.stereoTargetEye = StereoTargetEyeMask.None;

            UpdateDepthTextureActive();

            TransformChain = new TransformChain(transform, Camera.transform);
            Transformer = TransformChain.AddOrGet(TransformerTypeAndOrder.PositionOffset, false);

            foreach (Transform child in camClone.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var component in camClone.GetComponents<Behaviour>())
            {
                if (CameraBehavioursToDestroy.Contains(component.GetType().Name))
                {
                    DestroyImmediate(component);
                }
            }

            //Cloning post process stuff to make it controlable on a per camera basis
            //BloomShite.InstantiateBloomForCamera(UCamera).tag = null;
            //typeof(VisualEffectsController)
            //.GetField("_depthTextureEnabled", BindingFlags.Instance | BindingFlags.NonPublic)
            //.SetValue(camClone.GetComponent<VisualEffectsController>(), new BoolSO() { value = UCamera.depthTextureMode != DepthTextureMode.None });

            WorldCam = new GameObject("WorldCam").AddComponent<PositionableCam>();
            WorldCam.transform.parent = camClone.transform;

            Settings = new CameraSettings(this);
            Settings.Load(loadConfig);

            Middlewares = new[]
            {
                MakeMiddleware<MultiplayerMiddleware>(), 
                MakeMiddleware<FpsLimiterMiddleware>(),
                MakeMiddleware<SmoothFollowMiddleware>(),
                MakeMiddleware<ModMapExtensionsMiddleware>(),
                MakeMiddleware<Follow360Middleware>(),
                MakeMiddleware<FollowerMiddleware>(),
                MakeMiddleware<MovementScriptProcessorMiddleware>(),
                MakeMiddleware<VmcAvatarMiddleware>()
            };
            camClone.AddComponent<CamPostProcessor>().Init(this);

            LogInfo("loaded");
        }

        private IMHandler MakeMiddleware<T>() where T : CamMiddleware, IMHandler
        {
            return gameObject.AddComponent<T>().Init(this);
        }

        private void LateUpdate()
        {
            TimeSinceLastRender += Time.deltaTime;

            if (Camera && RenderTexture)
            {
                PrepareMiddleWareRender();
            }
        }

        internal void PrepareMiddleWareRender(bool forceRender = false)
        {
            if (!Camera || !RenderTexture || Middlewares == null)
            {
                return;
            }

            if (Middlewares.Any(t => !t.Pre() && !forceRender))
            {
                return;
            }

            TransformChain.Calculate();
            Camera.enabled = true;

            if (forceRender)
            {
                Camera.Render();
            }
        }

        internal void PostprocessCompleted()
        {
            Camera.enabled = false;

            foreach (var t in Middlewares)
            {
                t.Post();
            }

            TimeSinceLastRender = 0f;
#if FPSCOUNT
			renderedFrames++;
#endif
        }

        private void OnEnable()
        {
            // Force a render here, so we don't end up with a stale image after having just enabled this camera
            PrepareMiddleWareRender(true);
            if (PreviewImage != null)
            {
                PreviewImage.gameObject.SetActive(true);
            }

            ShowWorldCamIfNecessary();
        }

        private void OnDisable()
        {
            if (PreviewImage != null)
            {
                PreviewImage.gameObject.SetActive(false);
            }

            ShowWorldCamIfNecessary();
        }

        private void OnDestroy()
        {
            Destroying = true;
            gameObject.SetActive(false);

            if (PreviewImage != null)
            {
                Destroy(PreviewImage.gameObject);
            }

            if (_shield != null)
            {
                Destroy(_shield.gameObject);
            }

            Destroy(gameObject);
        }
    }
}