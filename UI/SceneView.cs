using BeatSaberMarkupLanguage.ViewControllers;
using Camera2.Enums;
using Camera2.Managers;
using JetBrains.Annotations;

namespace Camera2.UI
{
    public class SceneView : BSMLResourceViewController
    {
        public override string ResourceName => "Camera2.UI.Views.sceneView.bsml";

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