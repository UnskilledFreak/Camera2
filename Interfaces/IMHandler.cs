namespace Camera2.Interfaces
{
    internal interface IMHandler
    {
        // Prevents the cam from rendering this frame if returned false
        public bool Pre();
        public void Post();
        public void CamConfigReloaded();
    }
}