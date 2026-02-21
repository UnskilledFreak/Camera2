#if PRE_1_40_8
using Camera2.HarmonyPatches;
#endif
using Camera2.Managers;
using Camera2.Utils;
using HarmonyLib;
using IPA;
using System.Reflection;
using System.Threading.Tasks;
using Camera2.Behaviours.Spout;
using Camera2.Handler;
using Camera2.HarmonyPatches;
using Camera2.Installers;
using JetBrains.Annotations;
using SiraUtil.Zenject;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace Camera2
{
    [UsedImplicitly]
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        // if you want to know if the player is running Kinsi's original mod
        // or my modded version, check if this property exists, if so, it's the modded one :3
        [UsedImplicitly]
        public const bool ImModded = true;
        internal static IPALogger Log { get; private set; }
        internal static Material ShaderMatLuminanceKey;
        internal static Material ShaderMatOutline;
#if PRE_1_40_8
        internal static Material ShaderMatCa;
#endif
        internal static Shader ShaderVolumetricBlit;
        internal const string Name = "Camera 2.5";
        private const string ModdedVersion = "0.6.0";
        internal const string FullName = Name + " Mod " + ModdedVersion;
        internal static readonly string FullInfo = $"Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}\nby Kinsi55\nmodified by UnskilledFreak\nVersion {ModdedVersion}";

        private static Harmony Harmony { get; set; }

        [UsedImplicitly]
        [Init]
        public Plugin(IPALogger logger, Zenjector injector)
        {
            Log = logger;
            Log.Info($"{Name} mod {ModdedVersion} loading...");
            
            ConfigHandler.Instance = new ConfigHandler();

            injector.Install<AppInstaller>(Location.App, ConfigHandler.Instance);
            injector.Install<MenuInstaller>(Location.Menu);

            LoadShaders();
            SpoutLoader.LoadPlugin();
            Log.Info($"{Name} mod {ModdedVersion} loaded");
        }

#if PRE_1_40_8
        private static void LoadShaders()
        {
            void LoadNormalShaders(AssetBundle bundle)
            {
                ShaderMatLuminanceKey = new Material(bundle.LoadAsset<Shader>("luminancekey.shader"));
                // Why does this one need the full path and others don't? I have no fing idea!
                ShaderMatOutline = new Material(bundle.LoadAsset<Shader>("assets/bundledassets/cam2/texouline.shader"));
                ShaderMatCa = new Material(bundle.LoadAsset<Shader>("chromaticaberration.shader"));
                bundle.Unload(false);
            }

            void LoadVRShaders(AssetBundle bundle)
            {
                ShaderVolumetricBlit = bundle.LoadAsset<Shader>("volumetricblit.shader");
                bundle.Unload(false);
            }

            //LoadNormalShaders(AssetBundle.LoadFromFile("/home/freaky/Desktop/Development/Beat Saber Plugins/CS_BeatSaber_Camera2-0.6.109/Shaders/camera2utils"));
            //LoadVRShaders(AssetBundle.LoadFromFile("/home/freaky/Desktop/Development/Beat Saber Plugins/CS_BeatSaber_Camera2-0.6.109/Shaders/camera2utilsvr"));

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Camera2.Shaders.camera2utils"))
            {
                LoadNormalShaders(AssetBundle.LoadFromStream(stream));
            }
            
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Camera2.Shaders.camera2utilsvr"))
            {
                LoadVRShaders(AssetBundle.LoadFromStream(stream));
            }
#else
        private static void LoadShaders()
        {
            void LoadNormalShaders(AssetBundle bundle)
            {
                foreach (var assetName in bundle.GetAllAssetNames())
                {
                    Log.Info($"Loading asset {assetName}");
                }
                ShaderMatLuminanceKey = new Material(bundle.LoadAsset<Shader>("luminancekey.shader"));
                ShaderMatOutline = new Material(bundle.LoadAsset<Shader>("texouline.shader"));
                ShaderVolumetricBlit = bundle.LoadAsset<Shader>("volumetricblit.shader");
                bundle.Unload(false);
            }

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Camera2.Shaders.camera2utils");
            LoadNormalShaders(AssetBundle.LoadFromStream(stream));
#endif
        }

        [UsedImplicitly]
        [OnStart]
        public void OnApplicationStart()
        {
            Harmony = new Harmony("Kinsi55.BeatSaber.Cam2");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            GlobalFPSCap.Init();

            MovementScriptManager.LoadMovementScripts();

            SceneManager.activeSceneChanged += SceneUtil.OnActiveSceneChanged;

            // Marinate the Reflection stuff off-thread so the loading of cameras later is less blocking
            Task.Run(() =>
            {
                CustomNotesUtil.Reflect();
                if (ScoreSaber.Reflect())
                {
                    SDK.ReplaySources.Register(new ScoreSaber.ScoreSaberReplaySource());
                }
            });
        }

        [UsedImplicitly]
        [OnExit]
        public void OnApplicationQuit()
        {
            Harmony.UnpatchSelf();
        }
    }
}