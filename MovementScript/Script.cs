using System.Collections.Generic;
using System.IO;
using Camera2.Enums;
using Camera2.Handler;
using Camera2.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Camera2.MovementScript
{
    internal class Script
    {
        [JsonProperty("syncToSong")] 
        public bool SyncToSong { get; private set; }

        [JsonProperty("loop")] 
        public bool Loop { get; private set; } = true;

        public List<ScriptFrame> Frames { get; } = new List<ScriptFrame>();

        [JsonIgnore] 
        public float ScriptDuration { get; private set; }

        [JsonIgnore] 
        public string Name { get; set; } = "";
        
        [JsonIgnore]
        private string Path { get; set; } = "";

        private void PopulateTimes()
        {
            var time = 0f;
            foreach (var frame in Frames)
            {
                frame.StartTime = time;
                time = frame.TransitionEndTime = time + frame.Duration;
                time = frame.EndTime = time + frame.HoldTime;
            }

            ScriptDuration = time;
        }

        public static Script Load(string name)
        {
            var scriptPath = ConfigHandler.Instance.GetMovementScriptPath(name);
            if (!File.Exists(scriptPath))
            {
                return null;
            }

            var script = new Script
            {
                Name = name,
                Path = scriptPath,
            };

            var scriptContent = File.ReadAllText(scriptPath);
            // Not a Noodle movement script
            if (!scriptContent.Contains("Movements"))
            {
                JsonConvert.PopulateObject(scriptContent, script, JsonHelpers.LeanDeserializeSettings);
            }
            else
            {
                // Camera Plus movement script, we need to convert it...
                dynamic camPlusScript = JObject.Parse(scriptContent.ToLowerInvariant());

                script.SyncToSong = camPlusScript.activeinpausemenu != "true";

                foreach (var movement in camPlusScript.movements)
                {
                    script.Frames.Add(new ScriptFrame
                    {
                        Position = new Vector3((float)movement.startpos.x, (float)movement.startpos.y, (float)movement.startpos.z), 
                        RotationEuler = new Vector3((float)movement.startrot.x, (float)movement.startrot.y, (float)movement.startrot.z), 
                        FOV = (float)(movement.startpos.fov ?? 0f)
                    });

                    script.Frames.Add(new ScriptFrame
                    {
                        Position = new Vector3((float)movement.endpos.x, (float)movement.endpos.y, (float)movement.endpos.z),
                        RotationEuler = new Vector3((float)movement.endrot.x, (float)movement.endrot.y, (float)movement.endrot.z),
                        Duration = movement.duration,
                        HoldTime = movement.delay,
                        FOV = (float)(movement.endpos.fov ?? 0f),
                        Transition = movement.easetransition == "true" ? MoveType.Eased : MoveType.Linear
                    });
                }

                File.Move(script.Path, $"{script.Path}.cameraPlusFormat");
                script.Save();
            }

            script.PopulateTimes();

            return script;
        }

        public void Save()
        {
            File.WriteAllText(Path, JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }));
        }
    }
}