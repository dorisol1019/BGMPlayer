using Microsoft.Practices.Unity;
using Prism.Unity;
using BGMPlayer.WPF.Views;
using System.Windows;

namespace BGMPlayer.WPF
{
    class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();
        }
    }
}
