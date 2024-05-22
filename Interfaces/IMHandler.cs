namespace Camera2.Interfaces
{
    internal interface IMHandler {
        bool Pre();
        void Post();
        void CamConfigReloaded();
    }
}