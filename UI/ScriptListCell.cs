using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using Camera2.Extensions;
using Camera2.MovementScript;
using HMUI;
using JetBrains.Annotations;

namespace Camera2.UI;

internal class ScriptListCell
{
    public Script Script { get; }

    [UsedImplicitly]
    private string Name => Script.Name;

    [UsedImplicitly]
    private string Details
    {
        get
        {
            var details = new List<string>
            {
                $"{Script.Frames.Count} {(Script.Frames.Count == 1 ? "Step" : "Steps")}",
                $"{Script.ScriptDuration:0.##}s"
            };

            if (Script.Loop)
            {
                details.Add("Loop");
            }

            if (Script.SyncToSong)
            {
                details.Add("Synced");
            }

            if (!Script.IsPlayable)
            {
                details.Add("needs 2 steps".ToBSMLYellow());
            }

            return string.Join(" | ", details);
        }
    }

#pragma warning disable CS0649

    [UIComponent("bgContainer"), UsedImplicitly]
    private ImageView _bg;

#pragma warning restore CS0649

    public ScriptListCell(Script script)
    {
        Script = script;
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