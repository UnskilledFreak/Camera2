using UnityEngine;

namespace Camera2.Behaviours.Spout
{
    [AddComponentMenu("Spout/CameraSpoutSender")]
    public class CameraSpoutSender : AbstractSpoutSender
    {
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            SendCameraMode(source, destination);
        }
    }
}