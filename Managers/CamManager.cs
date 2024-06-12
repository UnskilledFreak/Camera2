using Camera2.Behaviours;
using Camera2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Camera2.Enums;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace Camera2.Managers
{
    internal static class CamManager
    {
        public static List<Cam2> Cams { get; } = new List<Cam2>();
        internal static CamerasViewport CustomScreen { get; private set; }
        public static int BaseCullingMask { get; internal set; }
        public static int ClearedBaseCullingMask { get; private set; }

        public static void Init()
        {
            ClearedBaseCullingMask = BaseCullingMask != 0 ? BaseCullingMask : SceneUtil.GetMainCameraButReally().GetComponent<Camera>().cullingMask;

            foreach (int mask in Enum.GetValues(typeof(VisibilityMasks)))
            {
                ClearedBaseCullingMask &= ~mask;
            }

            //Adding _THIS_IS_NORMAL so that ends up in the stupid warning Unity logs when having a SS overlay w/ active VR
            CustomScreen = new GameObject("Cam2_Viewport_THIS_IS_NORMAL").AddComponent<CamerasViewport>();

            LoadCameras();

            ScenesManager.Settings.Load();

            XRSettings.gameViewRenderMode = GameViewRenderMode.None;
            
            _ = new GameObject("Cam2_Positioner", typeof(CamPositioner));

            UI.SpaghettiUI.Init();
        }

        [CanBeNull]
        public static Cam2 GetCameraByName(string name) => Cams.FirstOrDefault(x => x.Name == name);

        public static void CompleteReload()
        {
            CustomScreen.GetComponent<CamerasViewport>().CompleteReload();
        }

        private static void LoadCameras(bool reload = false)
        {
            if (!Directory.Exists(ConfigUtil.CamsDir))
            {
                Directory.CreateDirectory(ConfigUtil.CamsDir);
            }

            var loadedNames = new List<string>();

            foreach (var cam in Directory.GetFiles(ConfigUtil.CamsDir, "*.json"))
            {
                try
                {
                    var name = Path.GetFileNameWithoutExtension(cam);

                    InitCamera(name, true, reload);

                    if (reload)
                    {
                        loadedNames.Add(name);
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Failed to load Camera {Path.GetFileName(cam)}");
                    Plugin.Log.Error(ex);
                }
            }

            if (reload)
            {
                foreach (var deletedCam in Cams.Where(x => !loadedNames.Contains(x.Name)))
                {
                    Object.Destroy(deletedCam);
                    Cams.Remove(deletedCam);
                }
            }

            if (Cams.Count == 0)
            {
                InitCamera("Main", false);
            }

            ApplyCameraValues(true);
        }

        public static void Reload()
        {
            LoadCameras(true);
            ScenesManager.Settings.Load();
            UI.SpaghettiUI.ScenesSwitchUI.Update();
        }

        /*
         * Unfortunately the Canvas Images cannot have their "layer" / z-index set to arbitrary numbers,
         * so we need to sort the cams by their set layer number and set the sibling index accordingly
         */
        public static void ApplyCameraValues(bool viewLayer = false, bool bitMask = false, bool worldCam = false, bool posRot = false)
        {
            var collection = viewLayer ? Cams.OrderBy(x => x.IsCurrentlySelectedInSettings ? int.MaxValue : x.Settings.Layer).AsEnumerable() : Cams;

            foreach (var cam in collection)
            {
                if (viewLayer)
                {
                    cam.PreviewImage.transform.SetAsLastSibling();
                }

                if (bitMask)
                {
                    cam.Settings.ApplyLayerBitmask();
                }

                if (worldCam)
                {
                    cam.ShowWorldCamIfNecessary();
                }

                if (posRot)
                {
                    cam.Settings.ApplyPositionAndRotation();
                }
            }
        }

        private static Cam2 InitCamera(string name, bool loadConfig = true, bool reload = false)
        {
            var cam = GetCameraByName(name);
            if (cam != null)
            {
                if (!reload)
                {
                    throw new Exception("unable to create new cam, it already exists");
                }

                cam.Settings.Reload();
                return cam;
            }

            cam = new GameObject($"Cam2_{name}").AddComponent<Cam2>();

            try
            {
                cam.Init(name, CustomScreen.AddNewView(), loadConfig);
            }
            catch
            {
                Object.DestroyImmediate(cam);
                throw;
            }

            Cams.Add(cam);

            //Newly added cameras should always be the last child and thus on top
            //ApplyCameraValues(viewLayer: true);

            return cam;
        }

        public static Cam2 AddNewCamera(string namePrefix = "Unnamed Camera")
        {
            var nameToUse = namePrefix;
            var i = 2;

            while (GetCameraByName(nameToUse) != null)
            {
                nameToUse = $"{namePrefix} {i++}";
            }

            return InitCamera(nameToUse, false);
        }

        public static void DeleteCamera(Cam2 cam)
        {
            if (!Cams.Contains(cam))
            {
                return;
            }
            
            Cams.Remove(cam);

            var cfgPath = ConfigUtil.GetCameraPath(cam.Name);

            Object.DestroyImmediate(cam);

            if (File.Exists(cfgPath))
            {
                File.Delete(cfgPath);
            }

            foreach (var x in ScenesManager.Settings.Scenes.Values.Where(x => x.Contains(cam.Name)))
            {
                x.RemoveAll(z => z == cam.Name);
            }

            foreach (var x in ScenesManager.Settings.CustomScenes.Values.Where(x => x.Contains(cam.Name)))
            {
                x.RemoveAll(z => z == cam.Name);
            }

            ScenesManager.Settings.Save();
        }
    }
}