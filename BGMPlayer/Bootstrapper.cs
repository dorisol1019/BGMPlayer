using Microsoft.Practices.Unity;
using Prism.Unity;
using BGMPlayer.Views;
using System.Windows;

namespace BGMPlayer
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
