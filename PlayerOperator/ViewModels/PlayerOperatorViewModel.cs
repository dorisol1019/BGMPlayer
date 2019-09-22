using BGMPlayer;
using BGMPlayerCore;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings.Notifiers;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Reactive.Bindings.Extensions;
using BGMList.Models;
using System.Collections;
using PlayerOperator.Models;
using BGMPlayerService;

namespace PlayerOperator.ViewModels
{
    public class PlayerOperatorViewModel : BindableBase
    {
        private IBGMPlayerService player;

        private ISelectedBGM selectedBGM;

        private IUserOperationNotification<BGM> playingBGMNotification;
        public PlayerOperatorViewModel(IBGMPlayerService bgmPlayerService, IAllBGMs allBGMs, ISelectedBGM selectedBGM, IUserOperationNotification<BGM> playingBGMNotification)
        {
            this.player = bgmPlayerService;
            this.selectedBGM = selectedBGM;
            this.playingBGMNotification = playingBGMNotification;

            IsBusy = BusyNotifier.ToReadOnlyReactiveProperty();
            IsIdle = BusyNotifier.Inverse().ToReadOnlyReactiveProperty();

            bgms = allBGMs.BGMs.Value.OrderBy(e => e.FileName, new CompareNaural()).ToList();

            BGMList = new ReactiveProperty<IEnumerable<string>>(bgms.Select(e => e.FileName));

            playlist = new ShuffledPlaylist(bgms);

            PlayCommand = IsIdle.ToReactiveCommand();
            PlayCommand.Subscribe(async () =>
            {
                await this.Play();
            });
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

            player.LoopCounter.Where(_ => player.State.Value == PlayingState.Playing)
                .Where(x => x > 0)
                .Where(x => x >= LoopNumber.Value + 1)
                .Where(_ => LoopOptionSelectedIndex.Value != 0)
                .Subscribe(async count =>
                {
                    if (IsShuffleChecked.Value)
                    {
                        if (!(playlist is ShuffledPlaylist))
                        {
                            playlist = new ShuffledPlaylist(bgms);
                        }
                    }
                    else if (IsNextChecked.Value)
                    {
                        if (!(playlist is OrderedPlaylist))
                        {
                            playlist = new OrderedPlaylist(bgms, playingBGMNotification.Notification.Value);
                        }
                    }

                    BGM nextBGM = playlist.Next();

                    await Play(nextBGM);
                });

            this.playingBGMNotification.Notification.Subscribe(bgm =>
            {
                if (IsShuffleChecked.Value)
                {
                    playlist = new ShuffledPlaylist(bgms);
                }
                else if (IsNextChecked.Value)
                {
                    playlist = new OrderedPlaylist(bgms, bgm);
                }
            });

            this.player.State.Subscribe(state =>
            {
                switch (player.State.Value)
                {
                    case PlayingState.Playing:
                        this.PauseOrRestartButtonContent.Value = "一時停止";
                        break;
                    case PlayingState.Stopping:
                        this.PauseOrRestartButtonContent.Value = "";
                        break;
                    case PlayingState.Pausing:
                        this.PauseOrRestartButtonContent.Value = "停止解除";
                        break;
                    default:
                        break;
                }
            });
        }

        private IPlaylist playlist;
        public ReactiveProperty<IEnumerable<string>> BGMList { get; }

        private List<BGM> bgms = null;
        public ICommand RestartCommand { get; }
        public ICommand CtrlRightCommand { get; }
        public ICommand CtrlLeftCommand { get; }
        public ICommand SpaceCommand { get; }
        public ICommand EnterCommand { get; }
        public ICommand MouseDoubleClickCommand { get; }
        
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
        private Task Play()
        {
            var bgm = bgms.FirstOrDefault(e => e.FileName == selectedBGM.selectedBGM.Value);
            playingBGMNotification.Notification.Value = bgm;
            return Play(bgm);
        }
        private async Task Play(BGM bgm)
        {
            if (IsBusy.Value) return;
            using (BusyNotifier.ProcessStart())
            {
                await player.Play(bgm);
            }

            ChangeVolume();
        }

        private void Stop()
        {
            player.Stop();
        }

        private void PauseOrRestart()
        {
            player.PauseOrReStart();
        }
        private void ChangeVolume()
        {
            player.ChangeVolume((int)Volume.Value);
        }
    }
}
