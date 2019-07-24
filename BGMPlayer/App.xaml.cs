using BGMPlayer.Views;
using Prism.Ioc;
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
            containerRegistry.RegisterSingleton<IBGMPlayerCore, BGMPlayerCore>();
            containerRegistry.RegisterSingleton<IBGMPlayerService, BGMPlayerService>();
            
        }

    }
}
