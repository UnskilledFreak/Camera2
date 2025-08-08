using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using Camera2.Behaviours;
using Camera2.Managers;
using HMUI;
using IPA.Utilities.Async;
using JetBrains.Annotations;

namespace Camera2.UI
{
    [ViewDefinition("Camera2.UI.Views.CameraMovementSettings.bsml")]
    [HotReload(RelativePathToLayout = "Views.CameraMovementSettings.bsml")]
    public class CameraMovementSettingsViewController : BSMLAutomaticViewController
    {
       
        internal Cam2 CurrentCam { get; private set; }

        [UIComponent("movementScripts")]
        [CanBeNull]
        public CustomCellListTableData scriptsList;

        [UsedImplicitly]
        [UIValue("scripts")]
        internal List<object> AvailableScripts { get; } = new List<object>();

        [UsedImplicitly]
        [UIAction("script-selected")]
        internal void SelectScript(TableView _, object obj)
        {
            var toggle = (ScriptToggle)obj;
            if (toggle == null)
            {
                return;
            }

            var tmp = CurrentCam.Settings.MovementScript.ScriptList.ToList();
            if (Exists(toggle))
            {
                tmp.Remove(toggle.Name);
            }
            else
            {
                tmp.Add(toggle.Name);
            }

            CurrentCam.Settings.MovementScript.ScriptList = tmp.ToArray();
            UpdateList();
        }

        internal void UpdateList()
        {
#if PRE_1_40_8
            scriptsList?.tableView.ClearSelection();
#else
            scriptsList?.TableView.ClearSelection();
#endif
            
            AvailableScripts.Clear();

            AvailableScripts.AddRange(
                MovementScriptManager.MovementScripts
                    .Select(x =>
                    {
                        var tmp = new ScriptToggle
                        {
                            Name = x.Name,
                        };
                        tmp.IsEnabled = Exists(tmp);
                        tmp.Enabled = tmp.IsEnabled
                                ? "<color=\"green\">in use \u2713</color>"
                                : "<color=\"red\">not used \u2717</color>";

                        return tmp;
                    })
                    //.OrderByDescending(x => x.IsEnabled)
                    //.ThenBy(x => x.Name)
                    .OrderBy(x => x.Name)
                    .ToList()
            );

            UnityMainThreadTaskScheduler.Factory.StartNew(() =>
            {
#if PRE_1_40_8
                scriptsList?.tableView.ReloadData();
                //scriptsList?.tableView.ScrollToCellWithIdx();
#else
                scriptsList?.TableView.ReloadData();
                //scriptsList?.TableView.ScrollToCellWithIdx();
#endif
            }).ConfigureAwait(false);
        }

        internal void SetCam(Cam2 newCam)
        {
            CurrentCam = newCam;
            UpdateList();
        }

        private bool Exists(ScriptToggle toggle) => CurrentCam != null && CurrentCam.Settings.MovementScript.ScriptList.Contains(toggle.Name);

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            //UpdateList();
        }
    }
}