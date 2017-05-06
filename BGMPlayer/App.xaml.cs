using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Prism.Mvvm;
using Microsoft.Practices.Unity;

namespace BGMPlayer
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
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
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
