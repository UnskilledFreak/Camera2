using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Camera2.Behaviours;
using Camera2.Enums;
using Camera2.Handler;
using Camera2.Middlewares;
using Camera2.MovementScript;
using JetBrains.Annotations;
using UnityEngine;
using CameraType = Camera2.Enums.CameraType;
using Random = System.Random;

namespace Camera2.Managers
{
    internal static class MovementScriptManager
    {
        internal const float EditorCamFov = 90f;

        private static ConfigHandler _config => ConfigHandler.Instance;
        internal static List<Script> MovementScripts { get; } = new List<Script>();
        internal static Cam2 EditorCam { get; private set; }
        private static readonly Random RandomSource = new Random();

        public static Script AddScript()
        {
            var script = Script.Create(GetUniqueName("New Script"));
            MovementScripts.Add(script);

            return script;
        }

        public static Script DuplicateScript(Script script)
        {
            var copy = script.Duplicate(GetUniqueName(script.Name + " Duplicate"));
            MovementScripts.Add(copy);

            return copy;
        }

        public static void DeleteScript(Script script)
        {
            MovementScripts.Remove(script);
            script.Delete();
            ReplaceInCameras(script.Name, null);
        }

        public static bool RenameScript(Script script, string newName)
        {
            newName = string.Concat(newName.Split(Path.GetInvalidFileNameChars())).Trim();
            if (newName.Length == 0 || newName == script.Name)
            {
                return false;
            }

            var isTaken = MovementScripts.Any(x => IsSameName(x.Name, newName)) || File.Exists(_config.GetMovementScriptPath(newName));
            if (isTaken && !IsSameName(script.Name, newName))
            {
                return false;
            }

            var oldName = script.Name;
            script.Rename(newName);
            ReplaceInCameras(oldName, newName);

            return true;
        }

        [CanBeNull]
        public static Script GetRandomFromPossibles(string[] names)
        {
            var possibles = MovementScripts.Where(x => x.IsPlayable && names.Contains(x.Name)).ToList();
            return possibles.Count == 0
                ? null
                : possibles[RandomSource.Next(possibles.Count)];
        }

        public static void LoadMovementScripts(bool reload = false)
        {
            if (reload)
            {
                MovementScripts.Clear();
            }

            foreach (var file in _config.GetAllMovementScriptFiles())
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var script = Script.Load(name);

                if (!script.IsPlayable)
                {
                    Plugin.Log.Warn($"Movement script {name} has less than two keyframes and will not be played");
                }

                Plugin.Log.Info($"Loaded Movement script {name}");
                Plugin.Log.Info($"Sync to song: {script.SyncToSong}");
                Plugin.Log.Info($"Duration: {script.ScriptDuration} ({script.Frames.Count} frames)");

                MovementScripts.Add(script);
            }
        }

        public static void StopScript(Cam2 cam)
        {
            foreach (var middleware in cam.Middlewares.OfType<MovementScriptProcessorMiddleware>())
            {
                middleware.StopScript();
            }
        }

        public static void SetScriptPaused(Cam2 cam, bool isPaused)
        {
            foreach (var middleware in cam.Middlewares.OfType<MovementScriptProcessorMiddleware>())
            {
                middleware.SetPaused(isPaused);
            }
        }

        public static Cam2 CreateEditorCam()
        {
            if (EditorCam != null)
            {
                return EditorCam;
            }

            var cam = CamManager.AddTemporaryCamera();

            cam.Settings.TargetPosition = new Vector3(1.93f, 2.32f, -2.45f);
            cam.Settings.TargetRotation = new Vector3(16.48f, -24.22f, 0.81f);
            cam.Settings.Type = CameraType.Positionable;
            cam.Settings.WorldCamVisibility = WorldCamVisibility.Visible;
            cam.Settings.FOV = EditorCamFov;
            cam.Settings.MovementScript.ScriptList = [];
            cam.Settings.MovementScript.FromOrigin = true;
            cam.Settings.MovementScript.EnableInMenu = true;
            cam.Settings.SetViewRect(2f / 3, 0, 1f / 3, 1f / 3);

            cam.Settings.ApplyPositionAndRotation();
            cam.Settings.ApplyLayerBitmask();
            cam.UpdateRenderTextureAndView();
            cam.gameObject.SetActive(true);

            EditorCam = cam;
            CamManager.ApplyCameraValues(viewLayer: true, worldCam: true);

            return cam;
        }

        public static void DestroyEditorCam()
        {
            if (EditorCam == null)
            {
                return;
            }

            CamManager.DeleteCamera(EditorCam);
            EditorCam = null;
        }

        private static string GetUniqueName(string name)
        {
            var nameToUse = name;
            var i = 2;

            while (MovementScripts.Any(x => IsSameName(x.Name, nameToUse)) || File.Exists(_config.GetMovementScriptPath(nameToUse)))
            {
                nameToUse = $"{name} {i++}";
            }

            return nameToUse;
        }

        private static bool IsSameName(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private static void ReplaceInCameras(string name, [CanBeNull] string newName)
        {
            foreach (var cam in CamManager.Cams)
            {
                var scripts = cam.Settings.MovementScript.ScriptList;
                if (!scripts.Contains(name))
                {
                    continue;
                }

                cam.Settings.MovementScript.ScriptList = newName == null
                    ? scripts.Where(x => x != name).ToArray()
                    : scripts.Select(x => x == name ? newName : x).ToArray();
                cam.Settings.Save();

                if (newName == null)
                {
                    StopScript(cam);
                }
            }
        }
    }
}