using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using IPA.Utilities;

namespace Camera2.Behaviours.Spout
{
    internal static class SpoutLoader
    {
        private static readonly string PluginsDirectory = Path.Combine(UnityGame.InstallPath, @"Beat Saber_Data\Plugins\x86_64\");
        private static readonly string DllPath = Path.Combine(PluginsDirectory, "KlakSpout.dll");
        private const string ResourceName = "Camera2.Resources.Spout.KlakSpout.dll";

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        public static void LoadPlugin()
        {
            if (!Directory.Exists(PluginsDirectory))
            {
                return;
            }

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
            {
                if (stream == null)
                {
                    Plugin.Log.Warn("Failed to load Spout stream");
                    return;
                }
                using (var fs = new FileStream(DllPath, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fs);
                }
            }

            var handle = LoadLibrary(DllPath);
            if (handle == IntPtr.Zero)
            {
                var errorCode = Marshal.GetLastWin32Error();
                Plugin.Log.Error($"Failed to load Spout DLL! Win32 Error Code: {errorCode}");
            }
            else
            {
                Plugin.Log.Notice("Spout loaded Successfully!");
            }
        }
    }
}