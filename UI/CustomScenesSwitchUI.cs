using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using Camera2.Managers;
using HMUI;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Camera2.UI
{
    class CustomScenesSwitchUI
    {
        [UIComponent("customScenesList")]
        [UsedImplicitly]
        private CustomCellListTableData _list;

        [UIValue("scenes")]
        private static List<object> Scenes => ScenesManager.Settings.CustomScenes.Keys
            .Select(x => new CustomSceneUIEntry(x))
            .Prepend(new CustomSceneUIEntry(null))
            .Cast<object>()
            .ToList();

        private void SwitchToCustomScene(TableView tableView, CustomSceneUIEntry row)
        {
            if (row.Name == null)
            {
                ScenesManager.LoadGameScene(forceReload: true);
                return;
            }

            ScenesManager.SwitchToCustomScene(row.Name);
        }

        public void Update(int setSelected = -1, bool reloadData = true)
        {
            if (_list == null || _list.tableView == null)
            {
                return;
            }

            if (reloadData)
            {
                _list.data = Scenes;
                _list.tableView.ReloadData();
            }

            if (setSelected <= -1)
            {
                return;
            }

            _list.tableView.SelectCellWithIdx(setSelected);
            _list.tableView.ScrollToCellWithIdx(setSelected, TableView.ScrollPositionType.Center, false);
        }
    }
}