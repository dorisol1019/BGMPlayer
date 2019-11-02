using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.Windows.Input;
using Prism.Commands;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using Reactive.Bindings.Notifiers;
using Reactive.Bindings.Extensions;
using System.Windows.Threading;
using Prism.Interactivity.InteractionRequest;
using System.IO;
using BGMPlayerCore;
using System.Collections;
using BGMList.Models;
using Prism.Services.Dialogs;
using PlayerOperator.Models;

namespace BGMPlayer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        public ReactiveProperty<string> Title { get; }

        public ReadOnlyReactivePropertySlim<bool> IsTopMostWindow { get; }

        const string _defaultTitle = "BGM鳴ら～すV3";
        

        public ICommand Shutdown { get; }
        public ICommand OpenFolderCommand { get; }

        public ICommand WindowClosedCommand { get; }

        private IAllBGMs allBGMs;

        public MainWindowViewModel(IBGMPlayerService bgmPlayerService, IAllBGMs allBGMs, ISettingService settingService , IDialogService dialogService)
        {
            Title = new ReactiveProperty<string>(_defaultTitle);
            IsTopMostWindow = settingService.IsTopMostWindow.ToReadOnlyReactivePropertySlim();

            Shutdown = new DelegateCommand(() => Application.Current.Shutdown());

            this.allBGMs = allBGMs;

            bgmPlayerService.State.Subscribe(state => {
                switch (state)
                {
                    case PlayingState.Playing:
                        this.Title.Value = $"再生中 : {bgmPlayerService.PlayingBGM.Value.FileName}";
                        break;
                    case PlayingState.Stopping:
                        this.Title.Value = _defaultTitle;
                        break;
                    case PlayingState.Pausing:
                        this.Title.Value = $"一時停止 : {bgmPlayerService.PlayingBGM.Value.FileName}";
                        break;
                    default:
                        break;
                }
            });

            PopUpVersionInfoCommand = new DelegateCommand(() =>
              dialogService.ShowDialog("VersionInfo", new DialogParameters(), (_) => { })
            );

            PopUpLibrarysInfoCommand = new DelegateCommand(() =>
                 dialogService.ShowDialog("LibrarysInfo", new DialogParameters(), (_) => { })
                );

            OpenFolderCommand = new DelegateCommand(() => OpenFolder());

            WindowClosedCommand = new DelegateCommand(() => bgmPlayerService.Dispose());
        }

        public DelegateCommand PopUpVersionInfoCommand { get; }
        public DelegateCommand PopUpLibrarysInfoCommand { get; }

        void OpenFolder()
        {
            var dig = new Models.OpenFolderDialog();
            dig.Show();

            var folderName = dig.FolderName;

            if (folderName == "") return;

            this.allBGMs.Refresh(folderName);
        }
    }
}
