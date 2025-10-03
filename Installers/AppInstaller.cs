using Camera2.Handler;
using JetBrains.Annotations;
using Zenject;

namespace Camera2.Installers;

[UsedImplicitly]
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
#if !PRE_1_40_8
        Container.BindInterfacesTo<FpfcHandler>().AsSingle().NonLazy();
#endif
    }
}