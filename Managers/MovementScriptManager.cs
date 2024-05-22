using Camera2.Configuration;
using Camera2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Camera2.Managers
{
    internal static class MovementScriptManager
    {
        public static Dictionary<string, MovementScript> MovementScripts { get; private set; } = new Dictionary<string, MovementScript>();

        public static void LoadMovementScripts(bool reload = false)
        {
            if (!Directory.Exists(ConfigUtil.MovementScriptsDir))
            {
                Directory.CreateDirectory(ConfigUtil.MovementScriptsDir);
            }
            else
            {
                var loadedNames = new List<string>();

                foreach (var cam in Directory.GetFiles(ConfigUtil.MovementScriptsDir, "*.json"))
                {
                    try
                    {
                        var name = Path.GetFileNameWithoutExtension(cam);

                        var script = MovementScript.Load(name);

                        if (script.Frames.Count < 2)
                        {
                            throw new Exception("Movement scripts must contain at least two keyframes");
                        }

                        Plugin.Log.Info($"Loaded Movement script {name}");
                        Plugin.Log.Info($"Sync to song: {script.SyncToSong}");
                        Plugin.Log.Info($"Duration: {script.ScriptDuration} ({script.Frames.Count()} frames)");

                        MovementScripts[name] = script;

                        if (reload && script != null)
                        {
                            loadedNames.Add(name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.Error($"Failed to load Movement script {Path.GetFileName(cam)}");
                        Plugin.Log.Error(ex);
                    }
                }

                if (!reload)
                {
                    return;
                }

                foreach (var deletedScript in MovementScripts.Where(x => !loadedNames.Contains(x.Key)))
                {
                    MovementScripts.Remove(deletedScript.Key);
                }
            }
        }
    }
}