using Camera2.Handler;
using Zenject;

namespace Camera2.Installers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class AppInstaller : Installer
{
    private readonly ConfigHandler _config;
    
    private AppInstaller(ConfigHandler config)
    {
        _config = config;
    }

    public override void InstallBindings()
    {
        Container.BindInstance(_config).AsSingle();
    }
}