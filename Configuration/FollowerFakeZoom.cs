namespace Camera2.Configuration
{
    internal class FollowerFakeZoom
    {
        public bool Enabled = false;
        public float NearestFOV = 10;
        public float FarthestFOV = 110;
        public float Distance = 3;

        public bool IsValid() => Enabled;

        public void Reset()
        {
            // err... maybe we need that later
        }
    }
}