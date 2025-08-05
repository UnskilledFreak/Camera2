namespace Camera2.Configuration;

public class SettingsSpout
{
    public bool Enabled { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string ChannelName { get; set; }

    public SettingsSpout()
    {
        Enabled = false;
        Width = 1920;
        Height = 1080;
        ChannelName = "Camera25";
    }
}