using System;
using BeatSaber.Settings;
using Camera2.Behaviours.Spout;
using UnityEngine;

namespace Camera2.Behaviours;

//
// implements a stub from ReeCamera Spout, thank you
// https://github.com/Reezonate/ReeCamera/blob/a24d1db6b08d72f1c89883226b4421639ed72f6a/Source/2_Core/Behaviors/MainCameraController.cs#L126
//
public class SpoutHandler
{
    private readonly TextureSpoutSender _spoutSender;
    private readonly Cam2 _cam;
    
    private bool _spoutTextureDirty;
    private bool _spoutInitialized;
    private RenderTexture _spoutTexture;
    private RenderTexture _oldTexture;
    private PositionableCam _positionableCam;

    internal SpoutHandler(Cam2 cam)
    {
        _cam = cam;
        _spoutSender = _cam.gameObject.AddComponent<TextureSpoutSender>();
        _spoutSender.blitShader = Plugin.ShaderVolumetricBlit;
        _spoutSender.enabled = false;
        MarkDirty();
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
        Dispose();

        if (_cam.Settings.Spout.Enabled)
        {
            Init();
        }
    }

    public void Init()
    {
        if (_spoutInitialized)
        {
            return;
        }
        
        _spoutTexture = new RenderTexture(
            Math.Max(1, _cam.Settings.Spout.Width), 
            Math.Max(1, _cam.Settings.Spout.Height), 
            32, 
            RenderTextureFormat.ARGB32)
        {
            filterMode = FilterMode.Bilinear, 
            antiAliasing = _cam.Settings.AntiAliasing
        };
        _spoutTexture.Create();

        _oldTexture = _cam.Camera.targetTexture;
        _positionableCam ??= _cam.WorldCam.GetComponent<PositionableCam>();
        _positionableCam.SetRenderTexture(_spoutTexture);
        _cam.PreviewImage.enabled = false;
        _cam.Camera.targetTexture = _spoutTexture;
        _cam.Camera.ResetProjectionMatrix();

        _spoutSender.channelName = _cam.Settings.Spout.ChannelName;
        _spoutSender.sourceTexture = _spoutTexture;
        _spoutSender.enabled = _cam.Settings.Spout.Enabled;
        _spoutInitialized = true;
    }

    private void Dispose()
    {
        if (!_spoutInitialized)
        {
            return;
        }
        
        _cam.Camera.targetTexture = _oldTexture;
        _cam.Camera.ResetProjectionMatrix();
        _positionableCam.SetSource(_cam);
        _cam.PreviewImage.enabled = true;
        _oldTexture = null;
        
        _spoutSender.sourceTexture = null;
        _spoutSender.enabled = false;
        _spoutInitialized = false;
            
        _spoutTexture.Release();
        _spoutTexture = null;
    }
}