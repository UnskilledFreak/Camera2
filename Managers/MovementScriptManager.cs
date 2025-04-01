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
        internal static List<Script> MovementScripts { get; } = new List<Script>();
        private static readonly Random RandomSource = new Random();
        
        public static void AddScript()
        {
            var name = "New Script";
            var counter = 0;
            while (MovementScripts.Any(x => x.Name == name))
            {
                counter++;
                name = "New Script " + counter;
            }
            var script = new Script { Name = name, };
            script.Save();
            MovementScripts.Add(script);
        }

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
                return;
            }

            var loadedNames = new List<string>();

            foreach (var cam in Directory.GetFiles(ConfigUtil.MovementScriptsDir, "*.json"))
            {
                var name = Path.GetFileNameWithoutExtension(cam);
                var script = Script.Load(name);

                if (script.Frames.Count < 2)
                {
                    Plugin.Log.Warn("Movement scripts must contain at least two keyframes");
                    continue;
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