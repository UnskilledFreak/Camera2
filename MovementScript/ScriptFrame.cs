using System.ComponentModel;
using Camera2.Enums;
using Camera2.JsonConverter;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Camera2.MovementScript
{
    internal class ScriptFrame
    {
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
}