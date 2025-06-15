using Camera2.UI;
using Zenject;

namespace Camera2.Installers;

// ReSharper disable once ClassNeverInstantiated.Global
internal class MenuInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<CameraListViewController>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<CameraSettingsViewController>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<CameraMovementSettingsViewController>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<CameraPreviewViewController>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<SceneViewController>().FromNewComponentAsViewController().AsSingle();
        Container.Bind<UI.SettingsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
        Container.Bind<UI.SceneFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
        Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
    }
}