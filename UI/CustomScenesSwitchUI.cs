using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using Camera2.Managers;
using HMUI;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Camera2.UI
{
    internal class CustomScenesSwitchUI
    {
        
#pragma warning disable CS0649

        [UIComponent("customScenesList")] [UsedImplicitly]
        private CustomCellListTableData _list;
        
#pragma warning restore CS0649
        
        [UIValue("scenes")]
        private static List<object> Scenes => ScenesManager.Settings.CustomScenes.Keys
            .Select(x => new CustomSceneUIEntry(x))
            .Prepend(new CustomSceneUIEntry(null))
            .Cast<object>()
            .ToList();

        [UsedImplicitly]
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
#if PRE_1_40_6
            if (_list == null || _list.tableView == null)
#else
            if (_list == null || _list.TableView == null)
#endif
            {
                return;
            }

            if (reloadData)
            {
#if PRE_1_40_6
                _list.data = Scenes;
                _list.tableView.ReloadData();
#else
                _list.Data = Scenes;
                _list.TableView.ReloadData();
#endif
            }

            if (setSelected <= -1)
            {
                return;
            }

#if PRE_1_40_6
            _list.tableView.SelectCellWithIdx(setSelected);
            _list.tableView.ScrollToCellWithIdx(setSelected, TableView.ScrollPositionType.Center, false);
#else
            _list.TableView.SelectCellWithIdx(setSelected);
            _list.TableView.ScrollToCellWithIdx(setSelected, TableView.ScrollPositionType.Center, false);
#endif
        }
    }
}