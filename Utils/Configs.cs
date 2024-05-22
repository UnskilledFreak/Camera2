using IPA.Utilities;
using System.IO;

namespace Camera2.Utils
{
    internal static class ConfigUtil
    {
        private static readonly string ConfigDir = Path.Combine(UnityGame.UserDataPath, "Camera2");

        public static readonly string ScenesCfg = Path.Combine(ConfigDir, "Scenes.json");
        public static readonly string CamsDir = Path.Combine(ConfigDir, "Cameras");
        public static readonly string MovementScriptsDir = Path.Combine(ConfigDir, "MovementScripts");

        public static string GetCameraPath(string name) => Path.Combine(CamsDir, $"{name}.json");

        public static string GetMovementScriptPath(string name) => Path.Combine(MovementScriptsDir, $"{name}.json");
    }
}