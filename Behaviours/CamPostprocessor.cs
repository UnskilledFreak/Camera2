using Camera2.Configuration;
using UnityEngine;

namespace Camera2.Behaviours
{
    internal class CamPostProcessor : MonoBehaviour
    {
        private static readonly int Threshold = Shader.PropertyToID("_Threshold");
        private static readonly int HasDepth = Shader.PropertyToID("_HasDepth");
        private static readonly int Width = Shader.PropertyToID("_Width");
        private static readonly int ChromaticAberration = Shader.PropertyToID("_ChromaticAberration");

        private Cam2 _cam;
        private CameraSettings Settings => _cam.Settings;

        public void Init(Cam2 cam)
        {
            _cam = cam;
        }

        public void OnDisable()
        {
            // ignore
        }

        public void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (enabled && Plugin.ShaderMatLuminanceKey)
            {
                Plugin.ShaderMatLuminanceKey.SetFloat(Threshold, Settings.PostProcessing.TransparencyThreshold);
                Plugin.ShaderMatLuminanceKey.SetFloat(HasDepth, _cam.Camera.depthTextureMode != DepthTextureMode.None ? 1 : 0);

                if (_cam.IsCurrentlySelectedInSettings)
                {
                    var tmp = RenderTexture.GetTemporary(src.width, src.height, 0);
                    Graphics.Blit(src, tmp, Plugin.ShaderMatLuminanceKey);

                    Plugin.ShaderMatOutline.SetFloat(Width, Settings.RenderScale * 10);
                    Graphics.Blit(tmp, dest, Plugin.ShaderMatOutline);
                    RenderTexture.ReleaseTemporary(tmp);
                }
                else
                {
                    Graphics.Blit(src, dest, Plugin.ShaderMatLuminanceKey);
                }

                if (Settings.PostProcessing.ChromaticAberrationAmount > 0)
                {
                    Plugin.ShaderMatCa.SetFloat(ChromaticAberration, Settings.PostProcessing.ChromaticAberrationAmount / 1000);
                    Graphics.Blit(dest, dest, Plugin.ShaderMatCa);
                }
            }
            else
            {
                Graphics.Blit(src, dest);
            }

            _cam.PostprocessCompleted();
        }
    }
}