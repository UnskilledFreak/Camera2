using System.Net;
using Camera2.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Camera2.Configuration
{
    internal class SettingsVmcAvatar : CameraSubSettings
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public VmcMode Mode { get; set; } = VmcMode.Disabled;

        public string Destination
        {
            get => Address.ToString();
            set
            {
                var stuff = value.Split(':');

                var parsedAddress = IPAddress.Parse(stuff[0]);
                var b = parsedAddress.GetAddressBytes();

                if (!IPAddress.IsLoopback(parsedAddress) && b[0] != 10 && (b[0] != 192 || b[1] != 168) && (b[0] != 172 || (b[1] < 16 || b[1] > 31)))
                {
                    Plugin.Log.Warn($"Tried to set public IP address ({value}) for camera {Settings.Cam.Name} as the VMC destination. As this is almost certainly not intended it was prevented");
                    return;
                }

                Address.Address = parsedAddress;
                Address.Port = stuff.Length == 2 ? ushort.Parse(stuff[1]) : 39540;
            }
        }

        [JsonIgnore]
        public IPEndPoint Address { get; set; } = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 39540);
    }
}