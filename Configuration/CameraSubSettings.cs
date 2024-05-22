using Newtonsoft.Json;

namespace Camera2.Configuration
{
    internal abstract class CameraSubSettings
    {
        [JsonIgnore] 
        protected CameraSettings Settings { get; private set; }

        private void Init(CameraSettings settings)
        {
            Settings = settings;
        }

        public static T GetFor<T>(CameraSettings settings) where T : CameraSubSettings, new()
        {
            var x = new T();
            x.Init(settings);
            return x;
        }
    }
}