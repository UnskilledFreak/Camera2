using UnityEngine;
using UnityEngine.UI;

namespace Camera2.Behaviours
{
    internal class CameraDesktopView : RawImage
    {
        public Cam2 Cam { get; private set; }
        public RectTransform Rect { get; private set; }

        private const float MinSizePct = 0.05f;

        public void SetPositionClamped(Vector2 delta, int[] matrix, bool writeToConfig = false)
        {
            /*
             * If you can simplify this I happily invite you to do so, this took me way too long lmao
             * This is probably possible in half the LOC, but I already spent way too much time on this
             */
            var vrc = Cam.Settings.ViewRect;
            var iMin = vrc.MinAnchor();
            var iMax = vrc.MaxAnchor();

            // Theoretical transform, ignoring bounds
            var oMin = new Vector2(iMin.x + (delta.x * matrix[0]), iMin.y + (delta.y * matrix[1]));
            var oMax = new Vector2(iMax.x + (delta.x * matrix[2]), iMax.y + (delta.y * matrix[3]));

            // Constraining min/max to stay within bounds and the size to stay the same
            var oMinConstrained = new Vector2(
                Mathf.Clamp(oMin.x, 0, 1 - (vrc.Width * matrix[2])),
                Mathf.Clamp(oMin.y, 0, 1 - (vrc.Height * matrix[3]))
            );
            var oMaxConstrained = new Vector2(
                Mathf.Clamp(oMax.x, vrc.Width * matrix[0], 1),
                Mathf.Clamp(oMax.y, vrc.Height * matrix[1], 1)
            );

            var clampW = matrix[0] | matrix[2];
            var clampH = matrix[1] | matrix[3];

            // Clamp output size to be at least N while staying in bounds
            var oMinClamped = new Vector2(
                matrix[0] == 0 ? iMin.x : Mathf.Clamp(oMinConstrained.x, 0, (clampW * oMaxConstrained.x) - MinSizePct),
                matrix[1] == 0 ? iMin.y : Mathf.Clamp(oMinConstrained.y, 0, (clampH * oMaxConstrained.y) - MinSizePct)
            );

            var oMaxClamped = new Vector2(
                matrix[2] == 0 ? iMax.x : Mathf.Clamp(oMaxConstrained.x, MinSizePct + (clampW * oMinConstrained.x), 1),
                matrix[3] == 0 ? iMax.y : Mathf.Clamp(oMaxConstrained.y, MinSizePct + (clampH * oMinConstrained.y), 1)
            );

            Rect.anchorMin = oMinClamped;
            Rect.anchorMax = oMaxClamped;

            if (!writeToConfig || delta == Vector2.zero)
            {
                return;
            }

            Cam.Settings.SetViewRect(oMinClamped.x, oMinClamped.y, oMaxClamped.x - oMinClamped.x, oMaxClamped.y - oMinClamped.y);
            Cam.Settings.Save();
        }

        public new void Awake()
        {
            Rect = transform as RectTransform;
            Rect.pivot = Rect.sizeDelta = new Vector2(0, 0);
        }

        public void SetSource(Cam2 cam)
        {
            Cam = cam;

            texture = cam.RenderTexture;

            Rect.anchorMin = cam.Settings.ViewRect.MinAnchor();
            Rect.anchorMax = cam.Settings.ViewRect.MaxAnchor();

            Rect.anchoredPosition = new Vector2(0, 0);
            gameObject.name = cam.Name;
        }
    }
}