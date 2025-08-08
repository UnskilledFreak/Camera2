using System;
using BeatSaber.Settings;
using Camera2.Behaviours.Spout;
using Camera2.Managers;
using UnityEngine;

namespace Camera2.Behaviours;

// implements a stub from ReeCamera Spout, thank you
// https://github.com/Reezonate/ReeCamera/blob/a24d1db6b08d72f1c89883226b4421639ed72f6a/Source/2_Core/Behaviors/MainCameraController.cs#L126
public class SpoutHandler : MonoBehaviour
{
    private TextureSpoutSender _spoutSender;
    private Cam2 _cam;

    private bool _spoutTextureDirty;
    private bool _spoutInitialized;
    private RenderTexture _spoutTexture;
    private RenderTexture _oldTexture;

    internal void Init(Cam2 cam)
    {
        _cam = cam;
        _spoutSender = gameObject.AddComponent<TextureSpoutSender>();
        _spoutSender.blitShader = Plugin.ShaderVolumetricBlit;
        _spoutSender.enabled = false;
    }

    public void MarkDirty()
    {
        _spoutTextureDirty = true;
    }

    public void UpdateIfDirty()
    {
        if (!_spoutTextureDirty)
        {
            return;
        }

        _spoutTextureDirty = false;
        Stop();

        if (_cam.Settings.Spout.Enabled)
        {
            Start();
        }
    }

    private void Awake()
    {
        //transform.localRotation = Quaternion.Euler(0, 0, 90);
    }

    private void Start()
    {
        if (_spoutInitialized)
        {
            return;
        }

        if (!_cam.Settings.Spout.Enabled)
        {
            return;
        }

        var scale = _cam.Settings.Spout.IgnoreRenderScale ? 1 : _cam.Settings.RenderScale;
        var width = Math.Max(1, (int)Math.Round(_cam.Settings.Spout.Width * scale));
        var height = Math.Max(1, (int)Math.Round(_cam.Settings.Spout.Height * scale));
        
        _cam.LogInfo($"starting spout at resolution: {width}x{height}");
        
        _spoutTexture = new RenderTexture(width, height, 32, RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Bilinear, 
            antiAliasing = _cam.Settings.AntiAliasing
        };
        _spoutTexture.Create();

        _oldTexture = _cam.Camera.targetTexture;
        _cam.PreviewImage.enabled = false;
        
        _cam.Camera.aspect = (float)width / height;
        _cam.Camera.targetTexture = _spoutTexture;
        //_cam.Camera.ResetProjectionMatrix();

        _spoutSender.channelName = _cam.Settings.Spout.ChannelName;
        _spoutSender.sourceTexture = _spoutTexture;
        _spoutSender.enabled = _cam.Settings.Spout.Enabled;
        _spoutInitialized = true;
        
        _cam.LogInfo("starting spout done");
    }

    private void Stop()
    {
        if (!_spoutInitialized)
        {
            return;
        }

        _cam.LogInfo("stopping spout");
        _cam.Camera.targetTexture = _oldTexture;
        _cam.Camera.ResetProjectionMatrix();
        _cam.PreviewImage.enabled = true;
        _oldTexture = null;

        _spoutSender.sourceTexture = null;
        _spoutSender.enabled = false;
        _spoutInitialized = false;

        _spoutTexture.Release();
        _spoutTexture = null;
        _cam.LogInfo("stopping spout done");
    }
}