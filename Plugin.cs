#if PRE_1_40_6
using Camera2.HarmonyPatches;
#endif
using Camera2.Managers;
using Camera2.Utils;
using HarmonyLib;
using IPA;
using System.Reflection;
using System.Threading.Tasks;
using Camera2.Handler;
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
        internal static IPALogger Log { get; private set; }
        internal static Material ShaderMatLuminanceKey;
        internal static Material ShaderMatOutline;
        internal static Material ShaderMatCa;
        internal static Shader ShaderVolumetricBlit;
        internal const string Name = "Camera 2.5";
        private const string ModdedVersion = "0.4.5";
        internal const string FullName = Name + " Mod " + ModdedVersion;
        internal static readonly string FullInfo = $"Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}\nby Kinsi55\nmodified by UnskilledFreak\nVersion {ModdedVersion}";

        private static Harmony Harmony { get; set; }

        [UsedImplicitly]
        [Init]
        public Plugin(IPALogger logger, Zenjector zenjector)
        {
            Log = logger;

            ConfigHandler.Instance = new ConfigHandler();

            zenjector.Install<AppInstaller>(Location.App, ConfigHandler.Instance);
            zenjector.Install<MenuInstaller>(Location.Menu);

            Log.Info($"{Name} mod {ModdedVersion} loading...");
            LoadShaders();
            Log.Info($"{Name} mod {ModdedVersion} loaded");
        }

#if PRE_1_40_6
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
        internal static void LoadShaders()
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

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Camera2.Shaders.latest.1_40_6_camera2utils");
            LoadNormalShaders(AssetBundle.LoadFromStream(stream));
#endif
        }

        [UsedImplicitly]
        [OnStart]
        public void OnApplicationStart()
        {
            Harmony = new Harmony("Kinsi55.BeatSaber.Cam2");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

#if V1_29_1
            GlobalFPSCap.Init();
#endif

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