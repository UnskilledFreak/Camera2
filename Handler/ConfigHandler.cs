using System.Collections.Generic;
using System.IO;
using IPA.Utilities;

namespace Camera2.Handler;

internal class ConfigHandler
{
    public static ConfigHandler Instance { get; set; }
    
    public string ScenesConfigFile { get; private set; }
    private readonly string _cameraDirectory;
    private readonly string _movementScriptsDirectory;
    
    public ConfigHandler()
    {
        var mainDirectory = Path.Combine(UnityGame.UserDataPath, "Camera2");
        _cameraDirectory = Path.Combine(mainDirectory, "Cameras");
        ScenesConfigFile = Path.Combine(mainDirectory, "Scenes.json");
        _movementScriptsDirectory = Path.Combine(mainDirectory, "MovementScripts");
        
        AutoCreateDirectory(mainDirectory);
        AutoCreateDirectory(_cameraDirectory);
        AutoCreateDirectory(_movementScriptsDirectory);
    }
    
    public string GetCameraPath(string name) => Path.Combine(_cameraDirectory, $"{name}.json");
    
    public string GetMovementScriptPath(string name) => Path.Combine(_movementScriptsDirectory, $"{name}.json");
    
    public IEnumerable<string> GetAllCameraFiles() => Directory.GetFiles(_cameraDirectory, "*.json");
    
    public IEnumerable<string> GetAllMovementScriptFiles() => Directory.GetFiles(_movementScriptsDirectory, "*.json");

    private static void AutoCreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}