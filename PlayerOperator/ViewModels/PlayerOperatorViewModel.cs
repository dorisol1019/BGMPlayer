using BGMList.Models;
using BGMPlayer;
using BGMPlayerCore;
using BGMPlayerService;
using PlayerOperator.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System.Collections;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Input;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PlayerOperator.ViewModels;

[SupportedOSPlatform("windows10.0.10240.0")]
public class PlayerOperatorViewModel : BindableBase, IDestructible
{
    private readonly IBGMPlayerService player;

    private readonly ISelectedBGM selectedBGM;

    private readonly IUserOperationNotification<BgmFilePath> playingBGMNotification;
    private readonly SystemMediaTransportControls systemMediaTransportControls;
    private readonly SystemMediaTransportControlsDisplayUpdater systemMediaTransportControlsDisplayUpdater;
    private readonly static InMemoryRandomAccessStream stream = GetIconRandomAccessStream();
    private static RandomAccessStreamReference GetThumbnail  => RandomAccessStreamReference.CreateFromStream(stream);

    private bool isVolumeSliderManipulating = false;
    public ICommand VolumeSliderManipulateStart { get; }
    public ICommand VolumeSliderManipulateComplete { get; }

    public PlayerOperatorViewModel(IBGMPlayerService bgmPlayerService, IAllBGMs allBGMs, ISelectedBGM selectedBGM, IUserOperationNotification<BgmFilePath> playingBGMNotification, ISettingService settingService
        , SystemMediaTransportControls systemMediaTransportControls)
    {
        this.systemMediaTransportControls = systemMediaTransportControls;
        systemMediaTransportControls.IsPauseEnabled = true;
        systemMediaTransportControls.IsPlayEnabled = true;
        systemMediaTransportControls.IsNextEnabled = true;
        systemMediaTransportControls.IsPreviousEnabled = true;
        systemMediaTransportControls.ButtonPressed += SystemMediaTransportControls_ButtonPressed;
        systemMediaTransportControlsDisplayUpdater = systemMediaTransportControls.DisplayUpdater;

        systemMediaTransportControlsDisplayUpdater.Thumbnail = GetThumbnail;
        systemMediaTransportControlsDisplayUpdater.Update();

        player = bgmPlayerService;
        this.selectedBGM = selectedBGM;
        this.playingBGMNotification = playingBGMNotification;

        IsBusy = BusyNotifier.ToReadOnlyReactiveProperty();
        IsIdle = BusyNotifier.Inverse().ToReadOnlyReactiveProperty();

        bgms = allBGMs.BGMs.Value.OrderBy(e => e.FileName, new CompareNaural()).ToList();
        allBGMs.BGMs.Subscribe(bgms =>
        {
            this.bgms = bgms.OrderBy(e => e.FileName, new CompareNaural()).ToList();
        });


        BGMList = new ReactiveProperty<IEnumerable<string>>(bgms.Select(e => e.FileName));

        playlist = new ShuffledPlaylist(bgms);

        PlayCommand = IsIdle.ToReactiveCommand();
        PlayCommand.Subscribe(async () =>
        {
            await Play();
        });
        StopCommand = new ReactiveCommand();
        StopCommand.Subscribe(Stop);

        PauseOrRestartButtonContent = new ReactiveProperty<string>("");
        PauseOrRestartCommand = new ReactiveCommand(PauseOrRestartButtonContent.Select(e => !string.IsNullOrEmpty(e)));
        PauseOrRestartCommand.Subscribe(PauseOrRestart);

        Volume = player.Volume.Where(_ => !isVolumeSliderManipulating).Select(volume => (double)volume).ToReactiveProperty();
        Volume.Subscribe(volume => player.ChangeVolume((int)volume));

        EnterCommand = PlayCommand;
        SpaceCommand = new DelegateCommand(() =>
        {
            PauseOrRestart();
        });

        MouseDoubleClickCommand = PlayCommand;

        IsTopMostWindow = settingService.IsTopMostWindow;
        TopMost = IsTopMostWindow.ToReadOnlyReactiveProperty();

        LoopNumber_string = new ReactiveProperty<string>("0", mode: ReactivePropertyMode.None);
        LoopNumber = LoopNumber_string.Select(e => int.TryParse(e, out int result) ? result : 0).ToReadOnlyReactiveProperty();

        LoopNumber_PreviewTextInput = new ReactiveProperty<TextCompositionEventArgs>(mode: ReactivePropertyMode.None);
        LoopNumber_PreviewTextInput.Subscribe((e) =>
        {
            bool canParse = false;
            {
                string? tmp = LoopNumber_string.Value + e.Text;
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
                        playlist = new ShuffledPlaylist(bgms, playingBGMNotification.Notification.Value);
                    }
                }
                else if (IsNextChecked.Value)
                {
                    if (!(playlist is OrderedPlaylist))
                    {
                        playlist = new OrderedPlaylist(bgms, playingBGMNotification.Notification.Value);
                    }
                }

                BgmFilePath nextBGM = playlist.Next();

                await Play(nextBGM);
            });

        this.playingBGMNotification.Notification.Subscribe(bgm =>
        {
            if (IsShuffleChecked.Value)
            {
                playlist = new ShuffledPlaylist(bgms, bgm);
            }
            else if (IsNextChecked.Value)
            {
                playlist = new OrderedPlaylist(bgms, bgm);
            }
        });

        player.State.Subscribe(state =>
        {
            switch (player.State.Value)
            {
                case PlayingState.Playing:
                    PauseOrRestartButtonContent.Value = "一時停止";
                    break;
                case PlayingState.Stopping:
                    PauseOrRestartButtonContent.Value = "";
                    break;
                case PlayingState.Pausing:
                    PauseOrRestartButtonContent.Value = "停止解除";
                    break;
                default:
                    break;
            }
        });

        VolumeSliderManipulateStart = new DelegateCommand(()=> isVolumeSliderManipulating = true);
        VolumeSliderManipulateComplete = new DelegateCommand(()=> isVolumeSliderManipulating = false);
    }

    private static InMemoryRandomAccessStream GetIconRandomAccessStream()
    {
        var uri = new Uri("pack://application:,,,/icon3.png", UriKind.Absolute);
        var resourceInfo = Application.GetResourceStream(uri);
        using var resourceInfoStream = resourceInfo.Stream;
        var array = new byte[resourceInfoStream.Length];
        resourceInfoStream.Read(array, 0, array.Length);
        var randomAccessStream = new InMemoryRandomAccessStream();
        randomAccessStream.WriteAsync(array.AsBuffer()).AsTask().Wait();
        return randomAccessStream;
    }

    private async void SystemMediaTransportControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
    {
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            if(args.Button == SystemMediaTransportControlsButton.Next || args.Button == SystemMediaTransportControlsButton.Previous)
            {
                if (IsShuffleChecked.Value)
                {
                    if (playlist is not ShuffledPlaylist)
                    {
                        playlist = new ShuffledPlaylist(bgms, playingBGMNotification.Notification.Value);
                    }
                }
                else if (IsNextChecked.Value)
                {
                    if (playlist is not OrderedPlaylist)
                    {
                        playlist = new OrderedPlaylist(bgms, playingBGMNotification.Notification.Value);
                    }
                }
            }

            if (args.Button == SystemMediaTransportControlsButton.Next)
            {
                BgmFilePath nextBGM = playlist.Next();
                await Play(nextBGM);
                return;
            }
            else if (args.Button == SystemMediaTransportControlsButton.Previous)
            {
                BgmFilePath nextBGM = playlist.Previous();
                await Play(nextBGM);
                return;
            }
            switch (player.State.Value)
            {
                case PlayingState.Playing:
                    PauseOrRestart();
                    systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case PlayingState.Stopping:
                    return;
                case PlayingState.Pausing:
                    PauseOrRestart();
                    systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
            }
        });
    }

    private IPlaylist playlist;
    public ReactiveProperty<IEnumerable<string>> BGMList { get; }

    private List<BgmFilePath> bgms;
    public ICommand SpaceCommand { get; }
    public ICommand EnterCommand { get; }
    public ICommand MouseDoubleClickCommand { get; }

    public ReactiveCommand PlayCommand { get; }
    public ReactiveCommand StopCommand { get; }
    public ReactiveCommand PauseOrRestartCommand { get; }
    public ReactiveProperty<double> Volume { get; }
    public ReactiveProperty<string> PauseOrRestartButtonContent { get; private set; }
    public ReadOnlyReactiveProperty<bool> TopMost { get; }
    public BooleanNotifier IsTopMostWindow { get; }

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
        BgmFilePath? bgm = bgms.FirstOrDefault(e => e.FileName == selectedBGM.selectedBGM.Value);
        if (bgm is null)
        {
            return Task.CompletedTask;
        }

        playingBGMNotification.Notification.Value = bgm;

        return Play(bgm);
    }

    private async Task Play(BgmFilePath bgm)
    {
        if (IsBusy.Value)
        {
            return;
        }

        using (BusyNotifier.ProcessStart())
        {
            await player.Play(bgm);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                systemMediaTransportControlsDisplayUpdater.Type = MediaPlaybackType.Music;
                systemMediaTransportControlsDisplayUpdater.MusicProperties.Title = bgm.FileName;
                systemMediaTransportControlsDisplayUpdater.Update();
                systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            });
        }

        player.ChangeVolume((int)Volume.Value);
    }

    [SupportedOSPlatform("windows10.0.10240.0")]

    private void Stop()
    {
        player.Stop();
        Application.Current.Dispatcher.Invoke(() =>
        {
            systemMediaTransportControlsDisplayUpdater.ClearAll();
            systemMediaTransportControlsDisplayUpdater.Thumbnail = GetThumbnail;
            systemMediaTransportControlsDisplayUpdater.Update();
            systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
        });
    }

    private void PauseOrRestart()
    {
        if (player.State.Value == PlayingState.Playing)
        {
            Application.Current.Dispatcher.Invoke(() =>
                        systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused
            );
        }
        else if (player.State.Value== PlayingState.Pausing)
        {
            Application.Current.Dispatcher.Invoke(() =>
            systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing);
        }
        player.PauseOrReStart();
    }

    public void Destroy()
    {
        stream?.Dispose();
    }
}
