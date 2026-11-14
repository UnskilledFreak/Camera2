using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Camera2.Managers;
using JetBrains.Annotations;
using UnityEngine.UI;

namespace Camera2.UI;

[ViewDefinition("Camera2.UI.Views.ScriptPreview.bsml")]
[HotReload(RelativePathToLayout = "Views.ScriptPreview.bsml")]
internal class ScriptPreviewViewController : BSMLAutomaticViewController
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

    [UsedImplicitly]
    [UIComponent("previewImage")]
    public RawImage image;

#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [UIAction("#post-parse")]
    [UsedImplicitly]
    internal void Clear()
    {
        if (image == null)
        {
            return;
        }

        image.texture = null;
        image.enabled = false;
    }

    private void Update()
    {
        if (image == null)
        {
            return;
        }

        var texture = MovementScriptManager.EditorCam == null ? null : MovementScriptManager.EditorCam.RenderTexture;
        if (image.texture != texture)
        {
            image.texture = texture;
        }

        image.enabled = texture != null;
    }
}