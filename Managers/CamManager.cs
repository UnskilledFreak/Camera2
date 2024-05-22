using Camera2.Behaviours;
using Camera2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Camera2.Enums;
using UnityEngine;
using UnityEngine.XR;
using Object = UnityEngine.Object;

namespace Camera2.Managers
{
    internal static class CamManager
    {
        public static Dictionary<string, Cam2> Cams { get; private set; } = new Dictionary<string, Cam2>();
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

            // todo :: UF :: possible memory leak when reloading or switching scenes
            new GameObject("Cam2_Positioner", typeof(CamPositioner));

            UI.SpaghettiUI.Init();
        }

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
                        loadedNames.Add(name);
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error($"Failed to load Camera {Path.GetFileName(cam)}");
                    Plugin.Log.Error(ex);
                }
            }

            if (reload)
            {
                foreach (var deletedCam in Cams.Where(x => !loadedNames.Contains(x.Key)))
                {
                    Object.Destroy(deletedCam.Value);
                    Cams.Remove(deletedCam.Key);
                }
            }

            if (Cams.Count == 0)
            {
                var cam = InitCamera("Main", false);
            }

            ApplyCameraValues(viewLayer: true);
        }

        public static void Reload()
        {
            LoadCameras(true);
            ScenesManager.Settings.Load();
        }

        /*
         * Unfortunately the Canvas Images cannot have their "layer" / z-index set to arbitrary numbers,
         * so we need to sort the cams by their set layer number and set the sibling index accordingly
         */
        public static void ApplyCameraValues(bool viewLayer = false, bool bitMask = false, bool worldCam = false, bool posRot = false)
        {
            var collection = viewLayer ? Cams.Values.OrderBy(x => x.IsCurrentlySelectedInSettings ? int.MaxValue : x.Settings.Layer).AsEnumerable() : Cams.Values;

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
            if (Cams.TryGetValue(name, out var cam))
            {
                if (reload)
                {
                    cam.Settings.Reload();
                    return cam;
                }

                throw new Exception("Already exists??");
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

            Cams[name] = cam;

            //Newly added cameras should always be the last child and thus on top
            //ApplyCameraValues(viewLayer: true);

            return cam;
        }

        public static Cam2 AddNewCamera(string namePrefix = "Unnamed Camera")
        {
            var nameToUse = namePrefix;
            var i = 2;

            while (Cams.ContainsKey(nameToUse))
                nameToUse = $"{namePrefix}{i++}";

            return InitCamera(nameToUse, false);
        }

        public static void DeleteCamera(Cam2 cam)
        {
            if (!Cams.Values.Contains(cam))
                return;

            if (Cams[cam.Name] != cam)
                return;

            Cams.Remove(cam.Name);

            var cfgPath = ConfigUtil.GetCameraPath(cam.Name);
            
            Object.DestroyImmediate(cam);

            if (File.Exists(cfgPath))
            {
                File.Delete(cfgPath);
            }

            foreach (var x in ScenesManager.Settings.Scenes.Values.Where(x => x.Contains(cam.Name)))
            {
                x.RemoveAll(x => x == cam.Name);
            }

            foreach (var x in ScenesManager.Settings.CustomScenes.Values.Where(x => x.Contains(cam.Name)))
            {
                x.RemoveAll(x => x == cam.Name);
            }

            ScenesManager.Settings.Save();
        }

        public static bool RenameCamera(Cam2 cam, string newName)
        {
            if (Cams.ContainsKey(newName))
            {
                return false;
            }

            if (!Cams.ContainsValue(cam))
            {
                return false;
            }

            newName = string.Concat(newName.Split(Path.GetInvalidFileNameChars())).Trim();

            if (newName.Length == 0)
            {
                return false;
            }

            var oldName = cam.Name;

            if (newName == oldName)
            {
                return true;
            }

            Cams[newName] = cam;
            Cams.Remove(oldName);

            foreach (var scene in ScenesManager.Settings.Scenes.Values.Where(scene => scene.Contains(oldName)))
            {
                scene.Add(newName);
                scene.Remove(oldName);
            }

            cam.Settings.Save();
            File.Move(cam.ConfigPath, ConfigUtil.GetCameraPath(newName));
            cam.Init(newName, rename: true);
            ScenesManager.Settings.Save();

            return true;
        }
    }
}