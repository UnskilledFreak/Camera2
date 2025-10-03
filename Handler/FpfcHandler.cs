using System;
using Camera2.HarmonyPatches;
using Camera2.Utils;
using SiraUtil.Tools.FPFC;
using UnityEngine;
using Zenject;

namespace Camera2.Handler;

internal class FpfcHandler : IDisposable
{
    public static FpfcHandler Instance;
    
    private readonly IFPFCSettings _fpfcSettings;
    private Transform _cameraTransform = null;

    public FpfcHandler(IFPFCSettings fpfcSettings)
    {
        _fpfcSettings = fpfcSettings;
        _fpfcSettings.Changed += OnSettingsChange;
        
        Instance ??= this;
    }

    private void OnSettingsChange(IFPFCSettings settings)
    {
        if (_cameraTransform == null)
        {
            try
            {
                var tmp = SceneUtil.GetMainCameraButReally().GetComponent("SiraUtil.Tools.FPFC.SimpleCameraController");
                //Plugin.Log.Info("cam: " + (tmp == null ? "null" : tmp.name));
                _cameraTransform = tmp?.transform;
            }
            catch (IndexOutOfRangeException e)
            {
                //Plugin.Log.Info("FPFC: main cam not ready yet...");
            }
        }
        HookFPFCToggle.SetFPFCActive(_cameraTransform, settings.Enabled);
    }

    public void Dispose()
    {
        _fpfcSettings.Changed -= OnSettingsChange;
    }
    
    public void ForceUpdate() => OnSettingsChange(_fpfcSettings);
}