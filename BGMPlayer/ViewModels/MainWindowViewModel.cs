using BGMList.Models;
using BGMPlayerCore;
using PlayerOperator.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace BGMPlayer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        public ReactiveProperty<string> Title { get; }

        public ReadOnlyReactivePropertySlim<bool> IsTopMostWindow { get; }

        private const string _defaultTitle = "BGM鳴ら～すV3";


        public ICommand Shutdown { get; }
        public ICommand OpenFolderCommand { get; }

        public ICommand WindowClosedCommand { get; }

        private readonly IAllBGMs allBGMs;

        public MainWindowViewModel(IBGMPlayerService bgmPlayerService, IAllBGMs allBGMs, ISettingService settingService, IDialogService dialogService)
        {
            Title = new ReactiveProperty<string>(_defaultTitle);
            IsTopMostWindow = settingService.IsTopMostWindow.ToReadOnlyReactivePropertySlim();

            Shutdown = new DelegateCommand(() => Application.Current.Shutdown());

            this.allBGMs = allBGMs;

            bgmPlayerService.State.Subscribe(state =>
            {
                switch (state)
                {
                    case PlayingState.Playing:
                        Title.Value = $"再生中 : {bgmPlayerService.PlayingBGM.Value.FileName}";
                        break;
                    case PlayingState.Stopping:
                        Title.Value = _defaultTitle;
                        break;
                    case PlayingState.Pausing:
                        Title.Value = $"一時停止 : {bgmPlayerService.PlayingBGM.Value.FileName}";
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

        private void OpenFolder()
        {
            var dig = new Models.OpenFolderDialog();
            dig.Show();

            string? folderName = dig.FolderName;

            if (folderName == "")
            {
                return;
            }

            allBGMs.Refresh(folderName);
        }
    }
}
