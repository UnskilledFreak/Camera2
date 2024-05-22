namespace Camera2.Configuration
{
    internal class SettingsPostProcessing : CameraSubSettings
    {
        public float TransparencyThreshold { get; set; }
        public float ChromaticAberrationAmount { get; set; } = 0f;

        private bool _forceDepthTexture;

        public bool ForceDepthTexture
        {
            get => _forceDepthTexture;
            set
            {
                _forceDepthTexture = value;
                Settings.Cam.UpdateDepthTextureActive();
            }
        }
    }
}