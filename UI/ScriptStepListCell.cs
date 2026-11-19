using BeatSaberMarkupLanguage.Attributes;
using Camera2.MovementScript;
using HMUI;
using JetBrains.Annotations;
using TMPro;

namespace Camera2.UI;

internal class ScriptStepListCell
{
    public int Index { get; }
    private readonly ScriptFrame _frame;

    [UsedImplicitly]
    private string Name => $"#{Index + 1}   {_frame.Position.x:0.##} / {_frame.Position.y:0.##} / {_frame.Position.z:0.##}";

    [UsedImplicitly]
    private string Details
    {
        get
        {
            var details = $"{_frame.Duration:0.##}s move | {_frame.HoldTime:0.##}s hold | {_frame.Transition}";
            return _frame.FOV > 0f
                ? $"{details} | FOV {_frame.FOV:0.#}"
                : details;
        }
    }

#pragma warning disable CS0649

    [UIComponent("bgContainer"), UsedImplicitly]
    private ImageView _bg;

    [UIComponent("nameText"), UsedImplicitly]
    private TextMeshProUGUI _nameText;

    [UIComponent("detailsText"), UsedImplicitly]
    private TextMeshProUGUI _detailsText;

#pragma warning restore CS0649

    public ScriptStepListCell(int index, ScriptFrame frame)
    {
        Index = index;
        _frame = frame;
    }

    internal void RefreshTexts()
    {
        if (_nameText == null || _detailsText == null)
        {
            return;
        }

        _nameText.text = Name;
        _detailsText.text = Details;
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