using Camera2.Enums;
using Camera2.JsonConverter;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Camera2.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GameObjects
    {
        private CameraSettings _parentSetting;

        [JsonConverter(typeof(StringEnumConverter)), JsonProperty("Walls")]
        private WallVisibility _walls = WallVisibility.Visible;

        [JsonProperty("Debris")] private bool _debris = true; //Maybe make Enum w/ Show / Hide / Linked like in Cam Plus
        [JsonProperty("UI")] private bool _ui = true;

        [JsonConverter(typeof(StringEnumConverterMigrateFromBool)), JsonProperty("Avatar")]
        private AvatarVisibility _avatar = AvatarVisibility.Visible;

        [JsonProperty("Floor")] private bool _floor = true;
        [JsonProperty("CutParticles")] private bool _cutParticles = true;

        [JsonConverter(typeof(StringEnumConverterMigrateFromBool)), JsonProperty("Notes")]
        private NoteVisibility _notes = NoteVisibility.Visible;

        [JsonProperty("EverythingElse")] private bool _everythingElse = true;
        [JsonProperty("Sabers")] private bool _sabers = true;

        internal GameObjects(CameraSettings parentSetting)
        {
            _parentSetting = parentSetting;
        }

        public GameObjects GetCopy()
        {
            var x = new GameObjects(null)
            {
                _walls = _walls,
                _debris = _debris,
                _ui = _ui,
                _avatar = _avatar,
                _floor = _floor,
                _cutParticles = _cutParticles,
                _notes = _notes
            };
            return x;
        }
        
        public WallVisibility Walls
        {
            get => _walls;
            set
            {
                _walls = value;
                _parentSetting?.ApplyLayerBitmask();
            }
        }

        public bool Debris
        {
            get => _debris;
            set
            {
                _debris = value;
                _parentSetting?.ApplyLayerBitmask();
            }
        }

        public bool UI
        {
            get => _ui;
            set
            {
                _ui = value;
                _parentSetting?.ApplyLayerBitmask();
            }
        }

        public AvatarVisibility Avatar
        {
            get => _avatar;
            set
            {
                _avatar = value;
                _parentSetting?.ApplyLayerBitmask();
            }
        }

        public bool Floor
        {
            get => _floor;
            set
            {
                _floor = value;
                _parentSetting?.ApplyLayerBitmask();
            }
        }

        public bool CutParticles
        {
            get => _cutParticles;
            set
            {
                _cutParticles = value;
                _parentSetting?.ApplyLayerBitmask();
            }
        }

        public NoteVisibility Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                _parentSetting?.ApplyLayerBitmask();
            }
        }

        public bool Sabers
        {
            get => _sabers;
            set
            {
                _sabers = value;
                _parentSetting.ApplyLayerBitmask();
            }
        }

        public bool EverythingElse
        {
            get => _everythingElse;
            set
            {
                _everythingElse = value;
                _parentSetting.ApplyLayerBitmask();
            }
        }
    }
}