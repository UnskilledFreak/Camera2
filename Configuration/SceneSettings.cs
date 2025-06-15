using Camera2.Managers;
using Camera2.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Camera2.Behaviours;
using Camera2.Enums;
using Camera2.Handler;
using IPA.Utilities;
using UnityEngine;

namespace Camera2.Configuration
{
    internal class ScenesSettings
    {
        [JsonIgnore]
        private static ConfigHandler _config => ConfigHandler.Instance;
        
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<SceneTypes, List<string>> Scenes = new Dictionary<SceneTypes, List<string>>();

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, List<string>> CustomScenes = new Dictionary<string, List<string>>();

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<KeyCode, string> CustomSceneBindings = new Dictionary<KeyCode, string>();

        public bool AutoSwitchFromCustom = false;
        
        private bool _wasLoaded;

        public void Load(List<Cam2> cams)
        {
            if (File.Exists(_config.ScenesConfigFile))
            {
                try
                {
                    JsonConvert.PopulateObject(File.ReadAllText(_config.ScenesConfigFile), this, JsonHelpers.LeanDeserializeSettings);
                    DeleteInvalidCams(cams);
                }
                catch (Exception ex)
                {
                    if (!_wasLoaded)
                    {
                        Plugin.Log.Error("Failed to load Scenes config, it has been reset:");
                        Plugin.Log.Error(ex);

                        if (File.Exists($"{_config.ScenesConfigFile}.corrupted"))
                        {
                            File.Delete($"{_config.ScenesConfigFile}.corrupted");
                        }

                        File.Move(_config.ScenesConfigFile, $"{_config.ScenesConfigFile}.corrupted");
                    }
                    else
                    {
                        System.Threading.Tasks.Task.Run(() => WinAPI.MessageBox(IntPtr.Zero, "It seems like the Formatting of your Scenes.json is invalid! It was not loaded.\n\nIf you cant figure out how to fix the formatting you can simply delete it which will recreate it on next load", Plugin.FullName, 0x30));
                        return;
                    }
                }
            }

            // Populate missing Scenes if the Scenes cfg was outdated
            foreach (SceneTypes type in Enum.GetValues(typeof(SceneTypes)))
            {
                if (!Scenes.ContainsKey(type))
                {
                    Scenes.Add(type, new List<string>());
                }
            }

            _wasLoaded = true;
            
            Save();

            ScenesManager.LoadGameScene(forceReload: true);
        }

        public void Save()
        {
            if (_wasLoaded)
            {
                File.WriteAllText(_config.ScenesConfigFile, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        private void DeleteInvalidCams(List<Cam2> cams)
        {
            var nameList = cams.Select(x => x.Name).ToList();

            var cleanedCustomScenes = new Dictionary<string, List<string>>();
            var cleanedScenes = new Dictionary<SceneTypes, List<string>>();

            foreach (var (name, camNames) in Scenes)
            {
                cleanedScenes.Add(name, camNames.Where(x => nameList.Contains(x)).ToList());
            }
            
            foreach (var (name, camNames) in CustomScenes)
            {
                cleanedCustomScenes.Add(name, camNames.Where(x => nameList.Contains(x)).ToList());
            }

            Scenes = cleanedScenes;
            CustomScenes = cleanedCustomScenes;
        }
    }
}