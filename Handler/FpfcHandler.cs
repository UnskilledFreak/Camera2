using System;
using SiraUtil.Tools.FPFC;
using Zenject;

namespace Camera2.Handler;

internal class FpfcHandler : IDisposable
{
    public static FpfcHandler Instance;
    public bool IsActive { get; private set; }
    private readonly IFPFCSettings _fpfcSettings;

    public FpfcHandler(IFPFCSettings fpfcSettings)
    {
        Plugin.Log.Info("FPFC: hello");
        _fpfcSettings = fpfcSettings;
        _fpfcSettings.Changed += OnSettingsChange;
        OnSettingsChange(_fpfcSettings);
        
        Instance ??= this;
    }

    private void OnSettingsChange(IFPFCSettings settings)
    {
        IsActive = settings.Enabled;
        Plugin.Log.Info($"FPFC: settings changed to {IsActive}");
    }

    public void Dispose()
    {
        _fpfcSettings.Changed -= OnSettingsChange;
    }
}