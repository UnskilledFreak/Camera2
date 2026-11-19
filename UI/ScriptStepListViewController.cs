using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using JetBrains.Annotations;

namespace Camera2.UI;

[ViewDefinition("Camera2.UI.Views.ScriptStepList.bsml")]
[HotReload(RelativePathToLayout = "Views.ScriptStepList.bsml")]
internal class ScriptStepListViewController : BSMLAutomaticViewController
{
    private static ScriptFlowCoordinator Coordinator => ScriptFlowCoordinator.Instance;

    private readonly List<ScriptStepListCell> _cells = new List<ScriptStepListCell>();

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

    [UsedImplicitly]
    [UIComponent("stepList")]
    public CustomCellListTableData list;

    [UsedImplicitly]
    [UIComponent("addButton")]
    public NoTransitionsButton addButton;

    [UsedImplicitly]
    [UIComponent("deleteButton")]
    public NoTransitionsButton deleteButton;

    [UsedImplicitly]
    [UIComponent("moveUpButton")]
    public NoTransitionsButton moveUpButton;

    [UsedImplicitly]
    [UIComponent("moveDownButton")]
    public NoTransitionsButton moveDownButton;

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [UsedImplicitly]
    private List<object> Steps => _cells.ToList<object>();

#if PRE_1_40_8
    private TableView Table => list.tableView;
#else
    private TableView Table => list.TableView;
#endif

    [UIAction("#post-parse")]
    [UsedImplicitly]
    internal void Refresh()
    {
        if (list == null)
        {
            return;
        }

        _cells.Clear();
        if (Coordinator.CurrentScript != null)
        {
            _cells.AddRange(Coordinator.CurrentScript.Frames.Select((frame, index) => new ScriptStepListCell(index, frame)));
        }

#if PRE_1_40_8
        list.data = Steps;
#else
        list.Data = Steps;
#endif
        Table.ReloadData();
        SelectCurrentCell(true);
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
        var index = Coordinator.CurrentStepIndex;
        var hasStep = index >= 0 && index < _cells.Count;

        addButton.interactable = Coordinator.CurrentScript != null;
        deleteButton.interactable = hasStep;
        moveUpButton.interactable = hasStep && index > 0;
        moveDownButton.interactable = hasStep && index < _cells.Count - 1;

        if (!hasStep)
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

    [UIAction("SelectStep")]
    [UsedImplicitly]
    private void SelectStep(TableView tableView, ScriptStepListCell row)
    {
        Coordinator.SelectStep(row.Index);
        SelectCurrentCell(false);
    }

    [UIAction("AddStep")]
    [UsedImplicitly]
    private void AddStep() => Coordinator.AddStep();

    [UIAction("DeleteStep")]
    [UsedImplicitly]
    private void DeleteStep() => Coordinator.DeleteStep();

    [UIAction("MoveStepUp")]
    [UsedImplicitly]
    private void MoveStepUp() => Coordinator.MoveStep(-1);

    [UIAction("MoveStepDown")]
    [UsedImplicitly]
    private void MoveStepDown() => Coordinator.MoveStep(1);

    [UIAction("TakePose")]
    [UsedImplicitly]
    private void TakePose() => Coordinator.TakePoseFromEditorCam();

    [UIAction("PlayScript")]
    [UsedImplicitly]
    private void PlayScript() => Coordinator.StartPlayback();

    [UIAction("PauseScript")]
    [UsedImplicitly]
    private void PauseScript() => Coordinator.TogglePause();

    [UIAction("StopScript")]
    [UsedImplicitly]
    private void StopScript() => Coordinator.StopPlayback();
}