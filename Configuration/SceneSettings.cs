using Camera2.Managers;
using Camera2.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Camera2.Enums;
using UnityEngine;

namespace Camera2.Configuration
{
    internal class ScenesSettings
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<SceneTypes, List<string>> Scenes = new Dictionary<SceneTypes, List<string>>();

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, List<string>> CustomScenes = new Dictionary<string, List<string>>();

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<KeyCode, string> CustomSceneBindings = new Dictionary<KeyCode, string>();

        public bool AutoswitchFromCustom = false;
        
        private bool _wasLoaded;

        public void Load()
        {
            if (File.Exists(ConfigUtil.ScenesCfg))
            {
                try
                {
                    JsonConvert.PopulateObject(File.ReadAllText(ConfigUtil.ScenesCfg), this, JsonHelpers.LeanDeserializeSettings);
                }
                catch (Exception ex)
                {
                    if (!_wasLoaded)
                    {
                        Plugin.Log.Error($"Failed to load Scenes config, it has been reset:");
                        Plugin.Log.Error(ex);

                        if (File.Exists($"{ConfigUtil.ScenesCfg}.corrupted"))
                        {
                            File.Delete($"{ConfigUtil.ScenesCfg}.corrupted");
                        }

                        File.Move(ConfigUtil.ScenesCfg, $"{ConfigUtil.ScenesCfg}.corrupted");
                    }
                    else
                    {
                        System.Threading.Tasks.Task.Run(() => WinAPI.MessageBox(IntPtr.Zero, "It seems like the Formatting of your Scenes.json is invalid! It was not loaded.\n\nIf you cant figure out how to fix the formatting you can simply delete it which will recreate it on next load", Plugin.Name, 0x30));
                        return;
                    }
                }
            }

            // Populate missing Scenes if the Scenes cfg was outdated
            foreach (SceneTypes foo in Enum.GetValues(typeof(SceneTypes)))
            {
                if (!Scenes.ContainsKey(foo))
                {
                    Scenes.Add(foo, new List<string>());
                }
            }

            _wasLoaded = true;
            
            Save();

            // AAaaaa I hate this being here. This needs to go to some better place IMO
            UI.SpaghettiUI.ScenesSwitchUI.Update();

            ScenesManager.LoadGameScene(forceReload: true);
        }

        public void Save()
        {
            if (_wasLoaded)
            {
                File.WriteAllText(ConfigUtil.ScenesCfg, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }
    }
}