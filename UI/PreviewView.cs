﻿using System.Collections;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using UnityEngine.UI;

namespace Camera2.UI
{
    internal class PreviewView : BSMLResourceViewController
    {
        private UnityEngine.RenderTexture _renderTexture;
        
        public override string ResourceName => "Camera2.UI.Views.camPreview.bsml";
        
#pragma warning disable CS0649
        
        [UIComponent("previewImage"), UsedImplicitly]
        public RawImage image;
        
#pragma warning restore CS0649

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