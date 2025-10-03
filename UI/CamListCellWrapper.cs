using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using Camera2.Behaviours;
using Camera2.Extensions;
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
        private string Details => string.Join(
            " | ",
            new List<string>
                {
                    $"{Cam.Settings.Type}",
                    SceneCount == 0
                        ? "0 Scenes".ToBSMLRed()
                        : $"{SceneCount} {(SceneCount == 1 ? "Scene" : "Scenes")}",
                    Cam.Settings.Spout.Enabled
                        ? "Spout2 output".ToBSMLGreen()
                        : ""
                }
                .Where(x => !string.IsNullOrWhiteSpace(x))
        );

        [UsedImplicitly]
        private string LayerUIText
        {
            get
            {
                var text = $"Layer {Cam.Settings.Layer}"; 
                var hasDuplicates = CamManager.Cams.Count(x => x.Settings.Layer == Cam.Settings.Layer) > 1;
                return hasDuplicates 
                    ? $"{text} ".ToBSMLYellow()
                    : text;
            }
        }

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