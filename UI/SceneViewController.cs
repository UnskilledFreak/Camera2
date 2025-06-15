using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Camera2.Enums;
using Camera2.Managers;
using JetBrains.Annotations;

namespace Camera2.UI
{
    [ViewDefinition("Camera2.UI.Views.SceneView.bsml")]
    [HotReload(RelativePathToLayout = "Views.SceneView.bsml")]
    public class SceneViewController : BSMLAutomaticViewController
    {
        [UsedImplicitly]
        public void SwitchToMenu()
        {
            ScenesManager.SwitchToScene(SceneTypes.Menu);
        }

        [UsedImplicitly]
        public void SwitchToMultiplayerMenu()
        {
            ScenesManager.SwitchToScene(SceneTypes.MultiplayerMenu);
        }

        [UsedImplicitly]
        public void SwitchToPlaying()
        {
            ScenesManager.SwitchToScene(SceneTypes.Playing);
        }

        [UsedImplicitly]
        public void SwitchToPlaying360()
        {
            ScenesManager.SwitchToScene(SceneTypes.Playing360);
        }

        [UsedImplicitly]
        public void SwitchToPlayingModmap()
        {
            ScenesManager.SwitchToScene(SceneTypes.PlayingModmap);
        }
        
        [UsedImplicitly]
        public void PlayingModmapNoMotion()
        {
            ScenesManager.SwitchToScene(SceneTypes.PlayingModmapNoMotion);
        }

        [UsedImplicitly]
        public void SwitchToPlayingMulti()
        {
            ScenesManager.SwitchToScene(SceneTypes.PlayingMulti);
        }

        [UsedImplicitly]
        public void SwitchToSpectatingMulti()
        {
            ScenesManager.SwitchToScene(SceneTypes.SpectatingMulti);
        }

        [UsedImplicitly]
        public void SwitchToReplay()
        {
            ScenesManager.SwitchToScene(SceneTypes.Replay);
        }

        [UsedImplicitly]
        public void SwitchToFPFC()
        {
            ScenesManager.SwitchToScene(SceneTypes.FPFC);
        }
    }
}