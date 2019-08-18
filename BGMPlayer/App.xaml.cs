using BGMList;
using BGMPlayer.Views;
using BGMPlayerCore;
using PlayerOperator;
using PlayerOperator.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System.Linq;
using System.Threading;
using System.Windows;

namespace BGMPlayer
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IBGMPlayerCoreApi, BGMPlayerCoreApi>();
            containerRegistry.RegisterSingleton<IBGMPlayerService, BGMPlayerService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<BGMListModule>(InitializationMode.WhenAvailable);
            moduleCatalog.AddModule<PlayerOperatorModule>(InitializationMode.WhenAvailable);
        }
    }
}
