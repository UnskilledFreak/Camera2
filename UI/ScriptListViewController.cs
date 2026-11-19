using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using Camera2.Managers;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace Camera2.UI;

[ViewDefinition("Camera2.UI.Views.ScriptList.bsml")]
[HotReload(RelativePathToLayout = "Views.ScriptList.bsml")]
internal class ScriptListViewController : BSMLAutomaticViewController
{
    private static ScriptFlowCoordinator Coordinator => ScriptFlowCoordinator.Instance;

    private readonly List<ScriptListCell> _cells = new List<ScriptListCell>();
    private bool _isRefreshing;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

    [UsedImplicitly]
    [UIComponent("scriptList")]
    public CustomCellListTableData list;

    [UsedImplicitly]
    [UIComponent("duplicateButton")]
    public NoTransitionsButton duplicateButton;

    [UsedImplicitly]
    [UIComponent("deleteButton")]
    public NoTransitionsButton deleteButton;

    [UsedImplicitly]
    [UIComponent("scriptSettings")]
    public RectTransform scriptSettings;

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [UsedImplicitly]
    private List<object> Scripts => _cells.ToList<object>();

#if PRE_1_40_8
    private TableView Table => list.tableView;
#else
    private TableView Table => list.TableView;
#endif

    [UsedImplicitly]
    internal string ScriptName
    {
        get => Coordinator?.CurrentScript?.Name ?? "";
        set
        {
            if (_isRefreshing)
            {
                return;
            }

            if (!Coordinator.RenameScript(value))
            {
                NotifyScriptSettings();
            }
        }
    }

    [UsedImplicitly]
    internal bool ScriptLoop
    {
        get => Coordinator?.CurrentScript?.Loop ?? false;
        set
        {
            if (!_isRefreshing)
            {
                Coordinator.SetLoop(value);
            }
        }
    }

    [UsedImplicitly]
    internal bool ScriptSyncToSong
    {
        get => Coordinator?.CurrentScript?.SyncToSong ?? false;
        set
        {
            if (!_isRefreshing)
            {
                Coordinator.SetSyncToSong(value);
            }
        }
    }

    [UsedImplicitly]
    internal string TestTarget
    {
        get => ScriptFlowCoordinator.TestTarget;
        set
        {
            if (!_isRefreshing)
            {
                Coordinator.SetTestTarget(value);
            }
        }
    }

    [UIAction("#post-parse")]
    [UsedImplicitly]
    internal void Refresh()
    {
        if (list == null)
        {
            return;
        }

        _cells.Clear();
        _cells.AddRange(MovementScriptManager.MovementScripts.OrderBy(x => x.Name).Select(x => new ScriptListCell(x)));

#if PRE_1_40_8
        list.data = Scripts;
#else
        list.Data = Scripts;
#endif
        Table.ReloadData();
        SelectCurrentCell(true);

        var hasScript = Coordinator.CurrentScript != null;
        duplicateButton.interactable = hasScript;
        deleteButton.interactable = hasScript;
        scriptSettings.gameObject.SetActive(hasScript);
        NotifyScriptSettings();
    }

    private void NotifyScriptSettings()
    {
        _isRefreshing = true;
        try
        {
            NotifyPropertyChanged(nameof(ScriptName));
            NotifyPropertyChanged(nameof(ScriptLoop));
            NotifyPropertyChanged(nameof(ScriptSyncToSong));
            NotifyPropertyChanged(nameof(TestTarget));
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    internal void RefreshCells()
    {
        foreach (var cell in _cells)
        {
            cell.RefreshTexts();
        }
    }

    private void SelectCurrentCell(bool scrollTo)
    {
        var index = _cells.FindIndex(x => x.Script == Coordinator.CurrentScript);
        if (index < 0)
        {
            Table.ClearSelection();
            return;
        }

        Table.SelectCellWithIdx(index);
        if (scrollTo)
        {
            Table.ScrollToCellWithIdx(index, TableView.ScrollPositionType.Center, false);
        }
    }

    [UIAction("SelectScript")]
    [UsedImplicitly]
    private void SelectScript(TableView tableView, ScriptListCell row) => Coordinator.SelectScript(row.Script);

    [UIAction("AddScript")]
    [UsedImplicitly]
    private void AddScript() => Coordinator.AddScript();

    [UIAction("DuplicateScript")]
    [UsedImplicitly]
    private void DuplicateScript() => Coordinator.DuplicateScript();

    [UIAction("DeleteScript")]
    [UsedImplicitly]
    private void DeleteScript() => Coordinator.DeleteScript();

    [UIAction("TestTargetAvatarHead")]
    [UsedImplicitly]
    private void TestTargetAvatarHead() => SetTestTarget(TargetTemplates.AvatarHead);

    [UIAction("TestTargetNalulunaAvatarHead")]
    [UsedImplicitly]
    private void TestTargetNalulunaAvatarHead() => SetTestTarget(TargetTemplates.NalulunaAvatarHead);

    [UIAction("TestTargetNalulunaCatHead")]
    [UsedImplicitly]
    private void TestTargetNalulunaCatHead() => SetTestTarget(TargetTemplates.NalulunaCatHead);

    [UIAction("TestTargetPointer")]
    [UsedImplicitly]
    private void TestTargetPointer() => SetTestTarget(TargetTemplates.Pointer);

    [UIAction("TestTargetLeftPanel")]
    [UsedImplicitly]
    private void TestTargetLeftPanel() => SetTestTarget(TargetTemplates.LeftPanel);

    [UIAction("TestTargetRightPanel")]
    [UsedImplicitly]
    private void TestTargetRightPanel() => SetTestTarget(TargetTemplates.RightPanel);

    [UIAction("TestTargetNone")]
    [UsedImplicitly]
    private void TestTargetNone() => SetTestTarget("");

    private void SetTestTarget(string target)
    {
        Coordinator.SetTestTarget(target);
        NotifyScriptSettings();
    }
}