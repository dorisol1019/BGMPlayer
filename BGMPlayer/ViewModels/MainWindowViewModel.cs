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
using BGMPlayer.Extension;
using System.Collections;

namespace BGMPlayer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        public ReactiveProperty<string> Title { get; }
        public ReactiveProperty<IEnumerable<string>> BGMList { get; }
        public ReactiveProperty<int> BGMSelectedIndex { get; }
        public ReactiveProperty<string> BGMSelectedItem { get; }

        const string _defaultTitle = "BGM鳴ら～すV3";
        
        private List<BGM> bgms = null;

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


        public ReactiveProperty<string> LoopNumber_string { get; }
        public ReadOnlyReactiveProperty<int> LoopNumber { get; }
        public ReactiveProperty<TextCompositionEventArgs> LoopNumber_PreviewTextInput { get; }
        public ReactiveProperty<int> LoopOptionSelectedIndex { get; }
        public ReactiveProperty<Visibility> LoopShuffleVisibility { get; }

        public ReactiveProperty<bool> IsShuffleChecked { get; }
        public ReactiveProperty<bool> IsNextChecked { get; }

        IBGMPlayerService player;
        public MainWindowViewModel(IBGMPlayerService player)
        {
            Title = new ReactiveProperty<string>(_defaultTitle);

            IsBusy = BusyNotifier.ToReadOnlyReactiveProperty();
            IsIdle = BusyNotifier.Inverse().ToReadOnlyReactiveProperty();

            string path = @"Playlist\";
#if DEBUG
            //path = @"testplaylist";
#endif

            this.player = player;

            bgms = GetBGMList(path);
            if (bgms.Count == 0)
            {
                MessageBox.Show("Playlistフォルダに音楽ファイルがありません\n" +
                    "音楽ファイルをPlaylistフォルダに入れてから起動して下さい", "Error!!");
            }
            BGMList = new ReactiveProperty<IEnumerable<string>>(bgms.Select(e=>e.FileName));
            BGMSelectedIndex = new ReactiveProperty<int>(0);
            BGMSelectedItem = new ReactiveProperty<string>("");

            Shutdown = new DelegateCommand(() => Application.Current.Shutdown());

            OpenFolderCommand = new DelegateCommand(OpenFolder);
            RestartCommand = new DelegateCommand(() =>
            {
                System.Diagnostics.Process.Start(App.ResourceAssembly.Location, "/restart");
                Application.Current.Shutdown();
            });

            PlayCommand = IsIdle.ToReactiveCommand();
            PlayCommand.Subscribe(async () => await this.Play());
            StopCommand = new ReactiveCommand();
            StopCommand.Subscribe(Stop);

            PauseOrRestartButtonContent = new ReactiveProperty<string>("");
            PauseOrRestartCommand = new ReactiveCommand(PauseOrRestartButtonContent.Select(e => !string.IsNullOrEmpty(e)));
            PauseOrRestartCommand.Subscribe(PauseOrRestart);

            Volume = new ReactiveProperty<double>(5);
            Volume.Subscribe(_ => ChangeVolume());

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
                  PauseOrRestart();
              });

            MouseDoubleClickCommand = PlayCommand;

            IsTopMostWindow = new ReactiveProperty<bool>(false);
            TopMost = IsTopMostWindow.ToReadOnlyReactiveProperty();

            WindowClosedCommand = new DelegateCommand(() => player.Dispose());

            LoopNumber_string = new ReactiveProperty<string>("0", mode: ReactivePropertyMode.None);
            LoopNumber = LoopNumber_string.Select(e => int.TryParse(e, out int result) ? result : 0).ToReadOnlyReactiveProperty();

            LoopNumber_PreviewTextInput = new ReactiveProperty<TextCompositionEventArgs>(mode: ReactivePropertyMode.None);
            LoopNumber_PreviewTextInput.Subscribe((e) =>
            {
                bool canParse = false;
                {
                    var tmp = LoopNumber_string.Value + e.Text;
                    canParse = uint.TryParse(tmp, out uint x);
                }
                e.Handled = !canParse;
            });
            LoopShuffleVisibility = new ReactiveProperty<Visibility>();
            LoopOptionSelectedIndex = new ReactiveProperty<int>(1);
            LoopOptionSelectedIndex.Subscribe(_ =>
            {
                if (LoopOptionSelectedIndex.Value == 1)
                {
                    LoopShuffleVisibility.Value = Visibility.Visible;
                }
                else
                {
                    LoopShuffleVisibility.Value = Visibility.Hidden;
                }
            });

            IsShuffleChecked = new ReactiveProperty<bool>(true);
            IsNextChecked = IsShuffleChecked.Inverse().ToReactiveProperty();

            //LoopCount = player.ObserveProperty(x => x.LoopCount).ToReactiveProperty();


            _interactionRequest = new InteractionRequest<INotification>();
            PopUpVersionInfoCommand = new DelegateCommand(() =>
              _interactionRequest.Raise(new Notification { Title = "BGM鳴ら～すV3について" })
            );
        }

        private InteractionRequest<INotification> _interactionRequest;
        public DelegateCommand PopUpVersionInfoCommand { get; }

        public InteractionRequest<INotification> InteractionRequest
        {
            get => _interactionRequest;
        }
        
        IDisposable loopCountDisposable;

        private Task Play()
        {
            return Play(bgms.FirstOrDefault(e => e.FileName == BGMSelectedItem.Value));
        }
        private async Task Play(BGM bgm)
        {

            if (IsBusy.Value) return;
            Title.Value = "ロード中…";
            using (BusyNotifier.ProcessStart())
            {
                await player.Play(bgm);
            }

            loopCountDisposable = player.LoopCounter.Subscribe(async i =>
             {
                 if (i <= 0) return;
                 int _index = -1;
                 if (LoopOptionSelectedIndex.Value == 0) return;
                 int loopN = LoopNumber.Value;
                 if (loopN + 1 > i) return;

                 if (IsShuffleChecked.Value)
                 {
                     var rand = new Random();

                     _index = rand.Next(BGMList.Value.Count());
                 }
                 else if (IsNextChecked.Value)
                 {
                     _index = selectedBGMIndex + 1;
                     if (BGMList.Value.Count() <= _index) _index = 0;
                 }
                 {
                     loopCountDisposable?.Dispose();
                     BGMSelectedItem.Value = bgms[_index].FileName;
                     await Play(bgms[_index]);
                 }
             });


            ChangeVolume();
            PauseOrRestartButtonContent.Value = "一時停止";
            Title.Value = $"再生中:{BGMSelectedItem.Value}";
        }

        private void Stop()
        {
            player.Stop();
            loopCountDisposable?.Dispose();
            PauseOrRestartButtonContent.Value = "";
            Title.Value = _defaultTitle;
        }

        private void PauseOrRestart()
        {
            if(player.IsPlaying.Value)
            {
                if(player.State.Value == PlayingState.Pausing)
                {
                    Restart();
                }
                else if (player.State.Value==PlayingState.Playing)
                {
                    Pause();
                }
            }
            player.PauseOrReStart();
        }

        private void Pause()
        {
            PauseOrRestartButtonContent.Value = "停止解除";
            Title.Value = $"一時停止中:{BGMSelectedItem.Value}";
        }

        private void Restart()
        {
            PauseOrRestartButtonContent.Value = "一時停止";
            Title.Value = $"再生中:{BGMSelectedItem.Value}";
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
            bgms = GetBGMList(folderName);
            BGMList.Value = bgms.Select(e=>e.FileName);
            selectedBGMIndex = -1;

        }

        private int selectedBGMIndex = -1;


        private string[] _extensionNames = new[] { ".mid", ".midi", ".wav", ".wave", ".mp3", ".ogg" };

        private List<BGM> GetBGMList(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException();
            var files = Directory.GetFiles(path);
            var enableFiles = new List<BGM>();
            foreach (var file in files)
            {
                foreach (var extensionName in _extensionNames)
                {
                    var extension = Path.GetExtension(file);
                    if (string.Compare(extension, extensionName, true) == 0)
                    {
                        FileExtensionType ext = FileExtensionType.other;
                        extension = extension.ToLower();
                        if (extension == ".mid" || extension == ".midi")
                            ext = FileExtensionType.midi;
                        if (extension == ".wav" || extension == ".wave")
                            ext = FileExtensionType.wave;
                        if (extension == ".mp3")
                            ext = FileExtensionType.mp3;
                        if (extension == ".ogg")
                            ext = FileExtensionType.ogg;

                        enableFiles.Add(new BGM(file, ext));
                    }
                }

            }
            enableFiles.Sort((e, f) => CompareExtension.CompareNatural(e.FileName, f.FileName));
            return enableFiles;
        }
    }
}
