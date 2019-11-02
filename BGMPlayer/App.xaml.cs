﻿using BGMList;
using BGMList.Models;
using BGMPlayer.ViewModels;
using BGMPlayer.Views;
using BGMPlayerCore;
using BGMPlayerService;
using PlayerOperator;
using PlayerOperator.Models;
using PlayerOperator.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System.IO;
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

        public override void Initialize()
        {
            if(!Directory.Exists("./Playlist"))
            {
                Directory.CreateDirectory("./Playlist");
            }
            base.Initialize();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<VersionInfoDialog, VersionInfoDialogViewModel>("VersionInfo");

            containerRegistry.RegisterInstance<IAllBGMs>(new AllBGMs());
            containerRegistry.RegisterInstance<ISelectedBGM>(new SelectedBGM());


            containerRegistry.RegisterSingleton<IBGMPlayerCoreApi, BGMPlayerCoreApi>();
            containerRegistry.RegisterSingleton<IBGMPlayerService, BGMPlayerService>();

            containerRegistry.RegisterInstance<IUserOperationNotification<BGM>>(new UserOperationNotification<BGM>());
            containerRegistry.Register<ISettingRepository, SettingRepository>();
            containerRegistry.RegisterSingleton<ISettingService, SettingService>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);
            moduleCatalog.AddModule<BGMListModule>(InitializationMode.WhenAvailable);
            moduleCatalog.AddModule<PlayerOperatorModule>(InitializationMode.WhenAvailable);
        }
    }
}
