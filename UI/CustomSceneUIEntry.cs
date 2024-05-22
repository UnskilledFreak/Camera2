using BeatSaberMarkupLanguage.Attributes;
using Camera2.Managers;
using HMUI;
using JetBrains.Annotations;

namespace Camera2.UI
{
    internal class CustomSceneUIEntry
    {
        public string Name { get; private set; }


        public CustomSceneUIEntry(string name)
        {
            Name = name;
            _bg = null;
        }

        private string InnerName => Name ?? "Switch to default scene";

        private bool Exists => ScenesManager.Settings.CustomScenes.ContainsKey(InnerName);
        [UsedImplicitly]
        private string CamNames => !Exists ? "" : string.Join(", ", ScenesManager.Settings.CustomScenes[InnerName]);
        [UsedImplicitly]
        private string CamCount
        {
            get
            {
                if (!Exists)
                {
                    return "";
                }

                var count = ScenesManager.Settings.CustomScenes[InnerName].Count;
                var str = $"{count} camera";

                if (count == 1)
                {
                    return str;
                }

                return str + "s";
            }
        }


        [UIComponent("bgContainer")] 
        [UsedImplicitly]
        private ImageView _bg;

        [UIAction("refresh-visuals")]
        [UsedImplicitly]
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