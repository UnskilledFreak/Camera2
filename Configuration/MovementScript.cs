using Camera2.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Camera2.Enums;
using Camera2.JsonConverter;
using UnityEngine;

namespace Camera2.Configuration
{
    internal class MovementScript
    {
        public class Frame
        {
            //[JsonConverter(typeof(StringEnumConverter))]
            //public PositionType posType = PositionType.Absolute;
            [JsonConverter(typeof(StringEnumConverter)), DefaultValue(MoveType.Linear)]
            public MoveType Transition = MoveType.Linear;

            [JsonConverter(typeof(Vector3Converter))]
            public Vector3 Position = Vector3.zero;

            [JsonIgnore] public Quaternion Rotation = Quaternion.identity;

            [JsonConverter(typeof(Vector3Converter)), JsonProperty("rotation")]
            public Vector3 RotationEuler
            {
                get => Rotation.eulerAngles;
                set => Rotation = Quaternion.Euler(value);
            }


            [DefaultValue(0f)] public float FOV;
            public float Duration;
            public float HoldTime;

            [JsonIgnore] public float StartTime;
            [JsonIgnore] public float TransitionEndTime;
            [JsonIgnore] public float EndTime;
        }

        [JsonProperty("syncToSong")] public bool SyncToSong { get; private set; }

        [JsonProperty("loop")] public bool Loop { get; private set; } = true;

        public List<Frame> Frames { get; private set; } = new List<Frame>();

        [JsonIgnore] public float ScriptDuration { get; private set; }

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

        public static MovementScript Load(string name)
        {
            var scriptPath = ConfigUtil.GetMovementScriptPath(name);
            if (!File.Exists(scriptPath))
            {
                return null;
            }

            var script = new MovementScript();

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
                    script.Frames.Add(new Frame
                    {
                        Position = new Vector3((float)movement.startpos.x, (float)movement.startpos.y, (float)movement.startpos.z), 
                        RotationEuler = new Vector3((float)movement.startrot.x, (float)movement.startrot.y, (float)movement.startrot.z), 
                        FOV = (float)(movement.startpos.fov ?? 0f)
                    });

                    script.Frames.Add(new Frame
                    {
                        Position = new Vector3((float)movement.endpos.x, (float)movement.endpos.y, (float)movement.endpos.z),
                        RotationEuler = new Vector3((float)movement.endrot.x, (float)movement.endrot.y, (float)movement.endrot.z),
                        Duration = movement.duration,
                        HoldTime = movement.delay,
                        FOV = (float)(movement.endpos.fov ?? 0f),
                        Transition = movement.easetransition == "true" ? MoveType.Eased : MoveType.Linear
                    });
                }

                File.Move(scriptPath, $"{scriptPath}.cameraPlusFormat");
                File.WriteAllText(scriptPath, JsonConvert.SerializeObject(script, Formatting.Indented, new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore }));
            }

            script.PopulateTimes();

            return script;
        }
    }
}