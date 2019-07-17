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

        }

        private Mutex mutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!e.Args.Any(f => f == "/restart"))
            {
                //Mutexクラスの作成
                //"MyName"の部分を適当な文字列に変える
                mutex = new Mutex(true, "DreamFool2017", out bool createdNew);
                if (createdNew == false)
                {
                    //ミューテックスの初期所有権が付与されなかったときは
                    //すでに起動していると判断して終了
                    MessageBox.Show("多重起動はできません。", "ERROR!!");
                    this.Shutdown();
                    return;
                }
            }
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (mutex != null)
            {
                //ミューテックスを解放する
                mutex.ReleaseMutex();
            }

            base.OnExit(e);
        }
    }
}
