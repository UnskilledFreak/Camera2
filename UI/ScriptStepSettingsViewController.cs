using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using Camera2.Enums;
using Camera2.MovementScript;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Camera2.UI;

[ViewDefinition("Camera2.UI.Views.ScriptStepSettings.bsml")]
[HotReload(RelativePathToLayout = "Views.ScriptStepSettings.bsml")]
internal class ScriptStepSettingsViewController : BSMLAutomaticViewController
{
    private static readonly string[] StepProperties =
    [
        nameof(PosX), nameof(PosY), nameof(PosZ),
        nameof(RotX), nameof(RotY), nameof(RotZ),
        nameof(Fov), nameof(Duration), nameof(HoldTime),
        nameof(Eased), nameof(IgnoreRotation), nameof(SmoothRotation)
    ];

    private const float CompactSliderWidth = 40f;

    private static ScriptFlowCoordinator Coordinator => ScriptFlowCoordinator.Instance;

    [CanBeNull]
    private static ScriptFrame Step => Coordinator?.CurrentStep;

    private bool _isRefreshing;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

    [UsedImplicitly]
    [UIComponent("stepSettings")]
    public RectTransform stepSettings;

    [UsedImplicitly]
    [UIComponent("timingSettings")]
    public RectTransform timingSettings;

    [UsedImplicitly]
    [UIComponent("emptyHint")]
    public TextMeshProUGUI emptyHint;

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [UsedImplicitly]
    internal float PosX
    {
        get => Step?.Position.x ?? 0f;
        set => SetPosition(0, value);
    }

    [UsedImplicitly]
    internal float PosY
    {
        get => Step?.Position.y ?? 0f;
        set => SetPosition(1, value);
    }

    [UsedImplicitly]
    internal float PosZ
    {
        get => Step?.Position.z ?? 0f;
        set => SetPosition(2, value);
    }

    [UsedImplicitly]
    internal float RotX
    {
        get => Step?.RotationEuler.x ?? 0f;
        set => SetRotation(0, value);
    }

    [UsedImplicitly]
    internal float RotY
    {
        get => Step?.RotationEuler.y ?? 0f;
        set => SetRotation(1, value);
    }

    [UsedImplicitly]
    internal float RotZ
    {
        get => Step?.RotationEuler.z ?? 0f;
        set => SetRotation(2, value);
    }

    [UsedImplicitly]
    internal float Fov
    {
        get => Step?.FOV ?? 0f;
        set
        {
            if (CanApply())
            {
                Step.FOV = value;
                Coordinator.StepValueChanged(true);
            }
        }
    }

    [UsedImplicitly]
    internal float Duration
    {
        get => Step?.Duration ?? 0f;
        set
        {
            if (CanApply())
            {
                Step.Duration = value;
                Coordinator.StepValueChanged(false);
            }
        }
    }

    [UsedImplicitly]
    internal float HoldTime
    {
        get => Step?.HoldTime ?? 0f;
        set
        {
            if (CanApply())
            {
                Step.HoldTime = value;
                Coordinator.StepValueChanged(false);
            }
        }
    }

    [UsedImplicitly]
    internal bool Eased
    {
        get => Step?.Transition == MoveType.Eased;
        set
        {
            if (CanApply())
            {
                Step.Transition = value ? MoveType.Eased : MoveType.Linear;
                Coordinator.StepValueChanged(false);
            }
        }
    }

    [UsedImplicitly]
    internal bool IgnoreRotation
    {
        get => Step?.OnFollowerIgnoreRotation ?? false;
        set
        {
            if (CanApply())
            {
                Step.OnFollowerIgnoreRotation = value;
                Coordinator.StepValueChanged(true);
            }
        }
    }

    [UsedImplicitly]
    internal bool SmoothRotation
    {
        get => Step?.OnFollowerSmoothRotation ?? false;
        set
        {
            if (CanApply())
            {
                Step.OnFollowerSmoothRotation = value;
                Coordinator.StepValueChanged(false);
            }
        }
    }

    [UIAction("#post-parse")]
    [UsedImplicitly]
    internal void Refresh()
    {
        if (stepSettings == null)
        {
            return;
        }

        foreach (var slider in timingSettings.GetComponentsInChildren<SliderSetting>(true))
        {
            SetSliderWidth(slider.transform, CompactSliderWidth);
        }

        var hasStep = Step != null;
        stepSettings.gameObject.SetActive(hasStep);
        emptyHint.gameObject.SetActive(!hasStep);
        emptyHint.text = Coordinator.CurrentScript == null
            ? "Add a Movement Script on the left to get started"
            : "This script has no steps yet, add one on the right";

        _isRefreshing = true;
        try
        {
            foreach (var property in StepProperties)
            {
                NotifyPropertyChanged(property);
            }
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private bool CanApply() => !_isRefreshing && Step != null;

    private static void SetSliderWidth(Transform setting, float width)
    {
        if (setting.Find("BSMLSlider") is RectTransform slider)
        {
            slider.sizeDelta = new Vector2(width, slider.sizeDelta.y);
        }

        if (setting.Find("Title") is RectTransform title)
        {
            title.offsetMax = new Vector2(-width, title.offsetMax.y);
        }
    }

    private void SetPosition(int axis, float value)
    {
        if (!CanApply())
        {
            return;
        }

        var position = Step.Position;
        position[axis] = value;
        Step.Position = position;
        Coordinator.StepValueChanged(true);
    }

    private void SetRotation(int axis, float value)
    {
        if (!CanApply())
        {
            return;
        }

        var rotation = Step.RotationEuler;
        rotation[axis] = value;
        Step.RotationEuler = rotation;
        Coordinator.StepValueChanged(true);
    }
}