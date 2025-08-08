using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using Camera2.Behaviours;
using Camera2.Managers;
using HMUI;
using JetBrains.Annotations;

namespace Camera2.UI
{
    internal class CamListCellWrapper
    {
        public Cam2 Cam { get; }

        private string Name => Cam.Name;

        private int SceneCount => ScenesManager.Settings.Scenes.Values.Count(x => x.Contains(Name)) + ScenesManager.Settings.CustomScenes.Values.Count(x => x.Contains(Name));

        [UsedImplicitly]
        private string Details => $"{Cam.Settings.Type}, assigned to {SceneCount} {(SceneCount == 1 ? "Scene" : "Scenes")}" + (Cam.Settings.Spout.Enabled ? " | <color=\"green\">Spout2 output \u2713</color>" : "");
        [UsedImplicitly]
        private string LayerUIText => $"Layer {Cam.Settings.Layer}{(CamManager.Cams.Count(x => x.Settings.Layer == Cam.Settings.Layer) > 1 ? " <color=#d5a145>⚠</color>" : "")}";

#pragma warning disable CS0649
        
        [UIComponent("bgContainer"), UsedImplicitly]
        private ImageView _bg;
        
#pragma warning restore CS0649

        public CamListCellWrapper(Cam2 cam)
        {
            Cam = cam;
        }

        [UIAction("refresh-visuals"), UsedImplicitly]
        public void Refresh(bool selected, bool highlighted)
        {
            var x = new UnityEngine.Color(0, 0, 0, 0.45f);

            if (selected || highlighted)
            {
                x.a = selected ? 0.9f : 0.6f;
            }

            _bg.color = x;
        }
    }
}