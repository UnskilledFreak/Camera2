using System.Collections;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using UnityEngine.UI;

namespace Camera2.UI
{
    [ViewDefinition("Camera2.UI.Views.CameraPreview.bsml")]
    [HotReload(RelativePathToLayout = "Views.CameraPreview.bsml")]
    internal class CameraPreviewViewController : BSMLAutomaticViewController
    {
        private UnityEngine.RenderTexture _renderTexture;
        
        [UIComponent("previewImage"), UsedImplicitly]
        public RawImage image = null;
        
        public void OnEnable()
        {
            if (image == null)
            {
                return;
            }

            _renderTexture = new UnityEngine.RenderTexture(UnityEngine.Screen.width, UnityEngine.Screen.height, 0);

            image.texture = _renderTexture;

            var imageTransform = image.transform;
            imageTransform.localPosition = new UnityEngine.Vector3(0, 0, -3.7f);
            imageTransform.localEulerAngles = new UnityEngine.Vector3(10, 180, 180);

            StartCoroutine(DoTheFunny());
        }

        public void OnDisable()
        {
            if (image)
            {
                image.texture = null;
            }

            if (_renderTexture == null)
            {
                return;
            }

            _renderTexture.Release();
            _renderTexture = null;
        }

        [UIAction("#post-parse")]
        [UsedImplicitly]
        private void Parsed()
        {
            image.transform.localEulerAngles = new UnityEngine.Vector3(180f, 0, 0);
            OnEnable();
        }

        private IEnumerator DoTheFunny()
        {
            while (isActiveAndEnabled)
            {
                yield return new UnityEngine.WaitForEndOfFrame();
                if (_renderTexture)
                {
                    UnityEngine.ScreenCapture.CaptureScreenshotIntoRenderTexture(_renderTexture);
                }

                yield return null;
                yield return null;
            }
        }
    }
}