using Camera2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Camera2.MovementScript;
using JetBrains.Annotations;

namespace Camera2.Managers
{
    internal static class MovementScriptManager
    {
        private static List<Script> MovementScripts { get; } = new List<Script>();
        private static readonly Random RandomSource = new Random();

        [CanBeNull]
        public static Script GetRandomFromPossibles(string[] names)
        {
            var possibles = MovementScripts.Where(x => names.Contains(x.Name)).ToList();
            return possibles.Count == 0 
                ? null 
                : possibles[RandomSource.Next(possibles.Count)];
        }

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

                        var script = Script.Load(name);

                        if (script.Frames.Count < 2)
                        {
                            throw new Exception("Movement scripts must contain at least two keyframes");
                        }

                        Plugin.Log.Info($"Loaded Movement script {name}");
                        Plugin.Log.Info($"Sync to song: {script.SyncToSong}");
                        Plugin.Log.Info($"Duration: {script.ScriptDuration} ({script.Frames.Count} frames)");

                        MovementScripts.Add(script);

                        if (reload)
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

                foreach (var deletedScript in MovementScripts.Where(x => !loadedNames.Contains(x.Name)).ToList())
                {
                    MovementScripts.Remove(deletedScript);
                }
            }
        }
    }
}