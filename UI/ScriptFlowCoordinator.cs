using System;
using System.Linq;
using Camera2.Behaviours;
using Camera2.Extensions;
using Camera2.Managers;
using Camera2.MovementScript;
using HMUI;
using JetBrains.Annotations;
using Zenject;

namespace Camera2.UI;

internal class ScriptFlowCoordinator : FlowCoordinator
{
    private const float DefaultStepDuration = 2f;

    internal static ScriptFlowCoordinator Instance { get; private set; }

    private MainFlowCoordinator _mainFlowCoordinator;
    private ScriptListViewController _scriptListViewController;
    private ScriptStepSettingsViewController _stepSettingsViewController;
    private ScriptStepListViewController _stepListViewController;
    private ScriptPreviewViewController _previewViewController;
    private ScriptPathVisualizer _pathVisualizer;
    private bool _isDirty;

    internal Script CurrentScript { get; private set; }
    internal int CurrentStepIndex { get; private set; } = -1;
    internal bool IsPlaying { get; private set; }
    internal bool IsPaused { get; private set; }

    [CanBeNull]
    internal ScriptFrame CurrentStep => CurrentScript != null && CurrentStepIndex >= 0 && CurrentStepIndex < CurrentScript.Frames.Count
        ? CurrentScript.Frames[CurrentStepIndex]
        : null;

    private static Cam2 EditorCam => MovementScriptManager.EditorCam;

    [Inject]
    [UsedImplicitly]
    public void Construct(
        MainFlowCoordinator mainFlowCoordinator,
        ScriptListViewController scriptListViewController,
        ScriptStepSettingsViewController stepSettingsViewController,
        ScriptStepListViewController stepListViewController,
        ScriptPreviewViewController previewViewController
    )
    {
        Instance = this;

        _mainFlowCoordinator = mainFlowCoordinator;
        _scriptListViewController = scriptListViewController;
        _stepSettingsViewController = stepSettingsViewController;
        _stepListViewController = stepListViewController;
        _previewViewController = previewViewController;
    }

    internal void SelectScript(Script script)
    {
        if (CurrentScript == script)
        {
            return;
        }

        StopPlayback();
        SaveIfDirty();
        SetCurrentScript(script);
        RefreshViews();
        ShowCurrentStep();
    }

    internal void AddScript() => SelectScript(MovementScriptManager.AddScript());

    internal void DuplicateScript()
    {
        if (CurrentScript == null)
        {
            return;
        }

        SaveIfDirty();
        SelectScript(MovementScriptManager.DuplicateScript(CurrentScript));
    }

    internal void DeleteScript()
    {
        if (CurrentScript == null)
        {
            return;
        }

        StopPlayback();
        _isDirty = false;
        MovementScriptManager.DeleteScript(CurrentScript);
        SetCurrentScript(GetFirstScript());
        RefreshViews();
        ShowCurrentStep();
    }

    internal bool RenameScript(string newName)
    {
        if (CurrentScript == null)
        {
            return false;
        }

        StopPlayback();
        if (!MovementScriptManager.RenameScript(CurrentScript, newName))
        {
            return false;
        }

        _scriptListViewController.Refresh();
        UpdateTitle();

        return true;
    }

    internal void SetLoop(bool loop)
    {
        if (CurrentScript == null || CurrentScript.Loop == loop)
        {
            return;
        }

        CurrentScript.Loop = loop;
        ScriptSettingChanged();
    }

    internal void SetSyncToSong(bool syncToSong)
    {
        if (CurrentScript == null || CurrentScript.SyncToSong == syncToSong)
        {
            return;
        }

        CurrentScript.SyncToSong = syncToSong;
        ScriptSettingChanged();
    }

    internal void SelectStep(int index)
    {
        if (CurrentScript == null || index == CurrentStepIndex)
        {
            return;
        }

        StopPlayback();
        CurrentStepIndex = index;
        _stepSettingsViewController.Refresh();
        UpdateTitle();
        UpdatePath();
        ShowCurrentStep();
    }

    internal void AddStep()
    {
        if (CurrentScript == null)
        {
            return;
        }

        var index = CurrentStepIndex + 1;
        var step = CurrentStep?.Clone() ?? new ScriptFrame();
        if (index > 0 && step.Duration <= 0f)
        {
            step.Duration = DefaultStepDuration;
        }

        if (EditorCam != null)
        {
            ApplyEditorCamPose(step);
        }

        StopPlayback();

        CurrentScript.Frames.Insert(index, step);
        CurrentStepIndex = index;
        StepsChanged();
    }

    internal void DeleteStep()
    {
        if (CurrentStep == null)
        {
            return;
        }

        StopPlayback();
        CurrentScript.Frames.RemoveAt(CurrentStepIndex);
        CurrentStepIndex = Math.Min(CurrentStepIndex, CurrentScript.Frames.Count - 1);
        StepsChanged();
    }

    internal void MoveStep(int direction)
    {
        var target = CurrentStepIndex + direction;
        if (CurrentStep == null || target < 0 || target >= CurrentScript.Frames.Count)
        {
            return;
        }

        StopPlayback();

        var frames = CurrentScript.Frames;
        (frames[CurrentStepIndex], frames[target]) = (frames[target], frames[CurrentStepIndex]);
        CurrentStepIndex = target;
        StepsChanged();
    }

    internal void StepValueChanged(bool poseChanged)
    {
        if (CurrentStep == null)
        {
            return;
        }

        StopPlayback();
        CurrentScript.PopulateTimes();
        _isDirty = true;
        _stepListViewController.RefreshCells();
        _scriptListViewController.RefreshCells();
        UpdatePath();

        if (poseChanged)
        {
            ShowCurrentStep();
        }
    }

    internal void TakePoseFromEditorCam()
    {
        var step = CurrentStep;
        if (step == null || EditorCam == null)
        {
            return;
        }

        ApplyEditorCamPose(step);
        StopPlayback();
        _stepSettingsViewController.Refresh();
        StepValueChanged(true);
    }

    private void ApplyEditorCamPose(ScriptFrame step)
    {
        if (IsPlaying)
        {
            step.Position = EditorCam.TransformChain.Position;
            step.RotationEuler = EditorCam.TransformChain.Rotation.eulerAngles.ClampToMiddlePointAngles();
        }
        else
        {
            step.Position = EditorCam.Settings.TargetPosition;
            step.RotationEuler = EditorCam.Settings.TargetRotation.ClampToMiddlePointAngles();
        }
    }

    internal void StartPlayback()
    {
        if (EditorCam == null || CurrentScript == null || !CurrentScript.IsPlayable)
        {
            return;
        }

        if (IsPlaying && IsPaused)
        {
            SetPaused(false);
            return;
        }

        MovementScriptManager.StopScript(EditorCam);
        EditorCam.Settings.MovementScript.ScriptList = [CurrentScript.Name];
        IsPlaying = true;
        IsPaused = false;
    }

    internal void TogglePause()
    {
        if (IsPlaying && EditorCam != null)
        {
            SetPaused(!IsPaused);
        }
    }

    private void SetPaused(bool isPaused)
    {
        IsPaused = isPaused;
        MovementScriptManager.SetScriptPaused(EditorCam, isPaused);
    }

    internal void StopPlayback()
    {
        if (!IsPlaying)
        {
            return;
        }

        IsPlaying = false;
        IsPaused = false;

        if (EditorCam == null)
        {
            return;
        }

        EditorCam.Settings.MovementScript.ScriptList = [];
        MovementScriptManager.StopScript(EditorCam);
        ShowCurrentStep();
    }

    private void SetCurrentScript(Script script)
    {
        CurrentScript = script;
        CurrentStepIndex = script != null && script.Frames.Count > 0 ? 0 : -1;
    }

    private static Script GetFirstScript() => MovementScriptManager.MovementScripts.OrderBy(x => x.Name).FirstOrDefault();

    private void ScriptSettingChanged()
    {
        CurrentScript.Save();
        _isDirty = false;
        _scriptListViewController.RefreshCells();
        UpdatePath();
    }

    private void StepsChanged()
    {
        CurrentScript.PopulateTimes();
        CurrentScript.Save();
        _isDirty = false;

        _stepListViewController.Refresh();
        _stepSettingsViewController.Refresh();
        _scriptListViewController.RefreshCells();
        UpdateTitle();
        UpdatePath();
        ShowCurrentStep();
    }

    private void SaveIfDirty()
    {
        if (!_isDirty || CurrentScript == null)
        {
            return;
        }

        CurrentScript.Save();
        _isDirty = false;
    }

    private void RefreshViews()
    {
        _scriptListViewController.Refresh();
        _stepListViewController.Refresh();
        _stepSettingsViewController.Refresh();
        UpdateTitle();
        UpdatePath();
    }

    private void UpdatePath()
    {
        if (_pathVisualizer != null)
        {
            _pathVisualizer.Show(CurrentScript, CurrentStepIndex);
        }
    }

    private void UpdateTitle()
    {
        var cardTitle = "";
        if (CurrentScript != null)
        {
            cardTitle = $"{CurrentScript.Name}";
        }

        if (CurrentStep != null)
        {
            cardTitle += $" | Step {CurrentStepIndex + 1} / {CurrentScript.Frames.Count}";
        }

        SetTitle(cardTitle);
    }

    private void ShowCurrentStep()
    {
        var step = CurrentStep;
        if (step == null || EditorCam == null)
        {
            return;
        }

        EditorCam.Settings.TargetPosition = step.Position;
        EditorCam.Settings.TargetRotation = step.RotationEuler;
        EditorCam.Settings.FOV = GetFovAtCurrentStep();
        EditorCam.Settings.ApplyPositionAndRotation();
    }

    private float GetFovAtCurrentStep()
    {
        for (var i = CurrentStepIndex; i >= 0; i--)
        {
            if (CurrentScript.Frames[i].FOV > 0f)
            {
                return CurrentScript.Frames[i].FOV;
            }
        }

        return MovementScriptManager.EditorCamFov;
    }

#if V1_29_1
    public
#else
    protected
#endif
        override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
    {
        MovementScriptManager.CreateEditorCam();
        if (_pathVisualizer == null)
        {
            _pathVisualizer = ScriptPathVisualizer.Create(EditorCam);
        }

        if (CurrentScript == null || !MovementScriptManager.MovementScripts.Contains(CurrentScript))
        {
            SetCurrentScript(GetFirstScript());
        }

        if (firstActivation)
        {
            showBackButton = true;
            ProvideInitialViewControllers(_stepSettingsViewController, _scriptListViewController, _stepListViewController, _previewViewController);
        }
        else
        {
            _scriptListViewController.Refresh();
            _stepListViewController.Refresh();
            _stepSettingsViewController.Refresh();
        }

        UpdateTitle();
        UpdatePath();
        ShowCurrentStep();
    }

#if V1_29_1
    public
#else
    protected
#endif
        override void BackButtonWasPressed(ViewController topViewController)
    {
        StopPlayback();
        SaveIfDirty();
        _previewViewController.Clear();
        MovementScriptManager.DestroyEditorCam();
        _pathVisualizer = null;
        ScenesManager.LoadGameScene(forceReload: true);
        _mainFlowCoordinator.DismissFlowCoordinator(this);
    }
}