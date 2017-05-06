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

namespace BGMPlayer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        public ReactiveProperty<string> Title { get; }
        public ReactiveProperty<IEnumerable<string>> BGMList { get; }
        public ReactiveProperty<int> BGMSelectedIndex { get; }
        public ReactiveProperty<string> BGMSelectedItem { get; }

        const string _defaultTitle = "BGM鳴ら～すV3";
        private BGMPlayerCore player;

        public ICommand Shutdown { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand RestartCommand { get; }
        public ICommand CtrlRightCommand { get; }
        public ICommand CtrlLeftCommand { get; }
        public ICommand SpaceCommand { get; }
        public ICommand EnterCommand { get; }
        public ICommand MouseDoubleClickCommand { get; }
        public ICommand WindowClosedCommand { get; }

        public ReactiveCommand PlayCommand { get; }
        public ReactiveCommand StopCommand { get; }
        public ReactiveCommand PauseOrRestartCommand { get; }
        public ReactiveCommand ChangeVolumeCommand { get; }
        public ReactiveProperty<double> Volume { get; }
        public ReactiveProperty<string> PauseOrRestartButtonContent { get; private set; }
        public ReadOnlyReactiveProperty<bool> TopMost { get; }
        public ReactiveProperty<bool> IsTopMostWindow { get; }

        private BusyNotifier BusyNotifier { get; } = new BusyNotifier();
        public ReadOnlyReactiveProperty<bool> IsBusy { get; }
        public ReadOnlyReactiveProperty<bool> IsIdle { get; }
        public MainWindowViewModel()
        {
            Title = new ReactiveProperty<string>(_defaultTitle);

            IsBusy = BusyNotifier.ToReadOnlyReactiveProperty();
            IsIdle = BusyNotifier.Inverse().ToReadOnlyReactiveProperty();

            string path = @"Playlist\";
#if DEBUG
            //path = @"testplaylist";
            path = @"Playlist\";
#endif

            player = new BGMPlayerCore((IntPtr)0, path);
            var bgmList = player.BGMNameList;
            if (bgmList.Count == 0)
            {
                MessageBox.Show("Playlistフォルダに音楽ファイルがありません\n" +
                    "音楽ファイルをPlaylistフォルダに入れてから起動して下さい", "Error!!");
            }
            BGMList = new ReactiveProperty<IEnumerable<string>>(bgmList);
            BGMSelectedIndex = new ReactiveProperty<int>(0);
            BGMSelectedItem = new ReactiveProperty<string>();

            Shutdown = new DelegateCommand(() => Application.Current.Shutdown());

            OpenFolderCommand = new DelegateCommand(OpenFolder);
            RestartCommand = new DelegateCommand(() =>
            {
                System.Diagnostics.Process.Start(App.ResourceAssembly.Location, "/restart");
                Application.Current.Shutdown();
            });

            PlayCommand = IsIdle.ToReactiveCommand();
            PlayCommand.Subscribe(async () => await Play());
            StopCommand = new ReactiveCommand();
            StopCommand.Subscribe(Stop);

            PauseOrRestartButtonContent = new ReactiveProperty<string>("");
            PauseOrRestartCommand = new ReactiveCommand(PauseOrRestartButtonContent.Select(e => !string.IsNullOrEmpty(e)));
            PauseOrRestartCommand.Subscribe(PauseOrRestart);

            Volume = new ReactiveProperty<double>(5);
            Volume.PropertyChanged += Volume_PropertyChanged;

            CtrlLeftCommand = new DelegateCommand(() =>
             {
                 if (Volume.Value > 0)
                 {
                     Volume.Value -= 1;
                 }
             }
            );
            CtrlRightCommand = new DelegateCommand(() =>
            {
                if (Volume.Value < 10)
                {
                    Volume.Value += 1;
                }
            }
            );

            EnterCommand = PlayCommand;
            SpaceCommand = new DelegateCommand(() =>
              {
                  if (player.IsPause)
                  {
                      Restart();
                  }
                  else
                  {
                      Pause();
                  }
              });

            MouseDoubleClickCommand = PlayCommand;//new DelegateCommand(async () => await Play());

            IsTopMostWindow = new ReactiveProperty<bool>(false);
            TopMost = IsTopMostWindow.ToReadOnlyReactiveProperty();

            WindowClosedCommand = new DelegateCommand(() => player.Dispose());
        }

        private void Volume_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ChangeVolume();
        }

        private async Task Play()
        {
            if (IsBusy.Value) return;
            using (BusyNotifier.ProcessStart())
            {
                int index = BGMSelectedIndex.Value;
                Stop();
                await Play(index);
            }
        }

        private async Task Play(int index)
        {
            await player.Play(index);
            ChangeVolume();
            PauseOrRestartButtonContent.Value = "一時停止";
            Title.Value = $"再生中:{player.SelectedBGM}";
        }

        private void Stop()
        {
            player.Stop();
            PauseOrRestartButtonContent.Value = "";
            Title.Value = _defaultTitle;
        }

        private void PauseOrRestart()
        {
            if (player.IsPlaying)
            {
                if (!player.IsPause)
                {
                    Pause();
                }
                else
                {
                    Restart();
                }
            }
        }

        private void Pause()
        {
            player.Pause();
            PauseOrRestartButtonContent.Value = "停止解除";
            Title.Value = $"一時停止中:{player.SelectedBGM}";
        }

        private void Restart()
        {
            player.ReStart();
            PauseOrRestartButtonContent.Value = "一時停止";
            Title.Value = $"再生中:{player.SelectedBGM}";
        }

        private void ChangeVolume()
        {
            player.ChangeVolume((int)Volume.Value);
        }

        void OpenFolder()
        {
            var dig = new Models.OpenFolderDialog();
            dig.Show();

            var folderName = dig.FolderName;
            player.Init(folderName);
            BGMList.Value = player.BGMNameList;
            BGMSelectedIndex.Value = 0;
            //selectedBGMIndex = -1;

        }


    }
}
