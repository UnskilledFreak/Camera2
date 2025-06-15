using Camera2.Enums;
using Camera2.Managers;
using JetBrains.Annotations;

namespace Camera2.UI
{
    internal class SceneToggle : NotifiableSettingsObj
    {
        internal CameraSettingsViewController Host;
        internal SceneTypes Type;

        [UsedImplicitly]
        internal bool Val
        {
            get => ScenesManager.Settings.Scenes[Type].Contains(Host.CamName);
            set
            {
                var x = ScenesManager.Settings.Scenes[Type];

                if (!value)
                {
                    x.RemoveAll(c => c == Host.CamName);
                }
                else if (!x.Contains(Host.CamName))
                {
                    x.Add(Host.CamName);
                }
            }
        }
    }
}