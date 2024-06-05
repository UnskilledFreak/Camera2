using Camera2.HarmonyPatches;
using Camera2.Managers;
using Camera2.Middlewares;
using Camera2.Utils;
using HarmonyLib;
using IPA;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace Camera2
{
    [UsedImplicitly]
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { [UsedImplicitly]get; private set; }
        internal static IPALogger Log { get; private set; }
        internal static Material ShaderMatLuminanceKey;
        internal static Material ShaderMatOutline;
        internal static Material ShaderMatCa;
        internal static Shader ShaderVolumetricBlit;
        internal const string Name = "Camera 2.5";
        
        private static Harmony Harmony { get; set; }

        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        [UsedImplicitly]
        [Init]
        public void Init(IPALogger logger)
        {
            Instance = this;
            Log = logger;

            Log.Info($"{Name} loaded");
            LoadShaders();
        }

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

#if DEV
			LoadNormalShaders(AssetBundle.LoadFromFile(@"D:\Unity Shit\Projects\AssetBundlePacker\Assets\StreamingAssets\camera2utils"));
			LoadVRShaders(AssetBundle.LoadFromFile(@"D:\Unity Shit\Projects\AssetBundlePacker\Assets\StreamingAssets\camera2utilsvr"));
#else
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Camera2.Shaders.camera2utils"))
            {
                LoadNormalShaders(AssetBundle.LoadFromStream(stream));
            }

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Camera2.Shaders.camera2utilsvr"))
            {
                LoadVRShaders(AssetBundle.LoadFromStream(stream));
            }
#endif
        }

        [UsedImplicitly]
        [OnStart]
        public void OnApplicationStart()
        {
            Harmony = new Harmony("Kinsi55.BeatSaber.Cam2");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            MovementScriptManager.LoadMovementScripts();
            GlobalFPSCap.Init();

            SceneManager.activeSceneChanged += SceneUtil.OnActiveSceneChanged;

            // Marinate the Reflection stuff off-thread so the loading of cameras later is less blocking
            Task.Run(() =>
            {
                ModMapExtensionsMiddleware.Reflect();
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