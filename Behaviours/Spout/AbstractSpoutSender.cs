using System;
using UnityEngine;

namespace Camera2.Behaviours.Spout
{
    public abstract class AbstractSpoutSender : MonoBehaviour
    {
        public string channelName = "SpoutSender";

        #region Private members

        // Unity stores render textures bottom-up on Direct3D, Spout receivers read the shared texture top-down.
        // Mirroring V while copying (v => 1 - v) makes the image arrive the right way up.
        private static readonly Vector2 FlipScale = new Vector2(1f, -1f);
        private static readonly Vector2 FlipOffset = new Vector2(0f, 1f);

        private IntPtr _plugin;
        private Texture2D _sharedTexture;
        private bool _sharedTextureInitialized;
        private bool _pluginInitialized;

        private void SendRenderTexture(Texture source)
        {
            if (source == null)
            {
                Plugin.Log.Error("Tried to send null texture");
                return;
            }
            
            InitializePlugin(source.width, source.height);
            if (!_pluginInitialized)
            {
                return;
            }

            InitializeSharedTexture();
            if (!_sharedTextureInitialized)
            {
                return;
            }

            var tempRT = RenderTexture.GetTemporary(_sharedTexture.width, _sharedTexture.height);
            Graphics.Blit(source, tempRT, FlipScale, FlipOffset);
            Graphics.CopyTexture(tempRT, _sharedTexture);
            RenderTexture.ReleaseTemporary(tempRT);
        }

        private void InitializePlugin(int width, int height)
        {
            if (_pluginInitialized)
            {
                return;
            }

            _plugin = PluginEntry.CreateSender(channelName, width, height);
            if (_plugin == IntPtr.Zero)
            {
                return; // Spout may not be ready.
            }

            _pluginInitialized = true;
        }

        private void InitializeSharedTexture()
        {
            if (_sharedTextureInitialized)
            {
                return;
            }

            var ptr = PluginEntry.GetTexturePointer(_plugin);
            if (ptr == IntPtr.Zero)
            {
                return;
            }

            _sharedTexture = Texture2D.CreateExternalTexture(
                PluginEntry.GetTextureWidth(_plugin),
                PluginEntry.GetTextureHeight(_plugin),
                TextureFormat.ARGB32, false, false, ptr
            );
            _sharedTexture.hideFlags = HideFlags.DontSave;
            _sharedTextureInitialized = true;
        }

        #endregion

        #region Protected members

        protected void SendTextureMode(RenderTexture texture)
        {
            SendRenderTexture(texture);
        }

        protected void SendCameraMode(RenderTexture source, RenderTexture destination)
        {
            SendRenderTexture(source);
            Graphics.Blit(source, destination);
        }

        #endregion

        #region MonoBehaviour implementation

        protected virtual void Update()
        {
            if (_pluginInitialized)
            {
                Util.IssuePluginEvent(PluginEntry.Event.Update, _plugin);
            }
        }

        protected virtual void OnDisable()
        {
            if (_pluginInitialized)
            {
                Util.IssuePluginEvent(PluginEntry.Event.Dispose, _plugin);
                _plugin = IntPtr.Zero;
                _pluginInitialized = false;
            }

            Util.Destroy(_sharedTexture);
            _sharedTextureInitialized = false;
        }

        #endregion
    }
}