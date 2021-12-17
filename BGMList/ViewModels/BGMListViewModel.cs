using BGMList.Models;
using BGMPlayer;
using BGMPlayerCore;
using BGMPlayerService;
using Prism.Mvvm;
using Reactive.Bindings;
using System.Collections;
using System.Reactive.Linq;
using System.Runtime.Versioning;
using System.Windows;
using Windows.Media;
using Windows.UI.ViewManagement;

namespace BGMList.ViewModels;

[SupportedOSPlatform("windows10.0.10240.0")]

public class BGMListViewModel : BindableBase
{
    public ReadOnlyReactiveProperty<List<string>> BGMs { get; }
    public ReactiveProperty<int> SelectedBGMIndex { get; set; }
    public ReactivePropertySlim<string> SelectedBGM { get; set; }

    public AsyncReactiveCommand PlayCommand { get; }

    public ReactiveCommand PauseOrRestartCommand { get; }

    public ReactiveCommand VolumeUpCommand { get; }
    public ReactiveCommand VolumeDownCommand { get; }

    private readonly IAllBGMs allBGMs;

    private readonly IBGMPlayerService bgmPlayerService;

    private SystemMediaTransportControls systemMediaTransportControls;
    private SystemMediaTransportControlsDisplayUpdater systemMediaTransportControlsDisplayUpdater;

    public BGMListViewModel(IBGMPlayerService bgmPlayerService, IAllBGMs allBGMs, ISelectedBGM selectedBGM, IUserOperationNotification<BgmFilePath> playingBGMNotification
        , SystemMediaTransportControls systemMediaTransportControls)
    {
        this.systemMediaTransportControls = systemMediaTransportControls;
        systemMediaTransportControlsDisplayUpdater = systemMediaTransportControls.DisplayUpdater;
        this.bgmPlayerService = bgmPlayerService;

        this.allBGMs = allBGMs;
        BGMs = allBGMs.BGMs.Select(e => e.Select(f => f.FileName).OrderBy(f => f, new CompareNaural()).ToList()).ToReadOnlyReactiveProperty();

        SelectedBGM = selectedBGM.selectedBGM;
        SelectedBGMIndex = new ReactiveProperty<int>(0);

        PlayCommand = new AsyncReactiveCommand();
        PlayCommand.Subscribe(async () =>
        {
            BgmFilePath? bgm = allBGMs.BGMs.Value.First(e => e.FileName == SelectedBGM.Value);
            playingBGMNotification.Notification.Value = bgm;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                systemMediaTransportControlsDisplayUpdater.Type = MediaPlaybackType.Music;
                systemMediaTransportControlsDisplayUpdater.MusicProperties.Title = bgm.FileName;
                systemMediaTransportControlsDisplayUpdater.Update();
                systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
            });
            await bgmPlayerService.Play(bgm);
        });

        PauseOrRestartCommand = new ReactiveCommand();
        PauseOrRestartCommand.Subscribe(() =>
        {
            if (bgmPlayerService.State.Value == PlayingState.Playing)
            {
                Application.Current.Dispatcher.Invoke(() =>
                            systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused
                );
            }
            else if (bgmPlayerService.State.Value == PlayingState.Pausing)
            {
                Application.Current.Dispatcher.Invoke(() =>
                systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing);
            }
            bgmPlayerService.PauseOrReStart();
        }
        );

        VolumeUpCommand = new ReactiveCommand();
        VolumeUpCommand.Subscribe(() =>
        {
            bgmPlayerService.ChangeVolume(bgmPlayerService.Volume.Value + 1);
        });

        VolumeDownCommand = new ReactiveCommand();
        VolumeDownCommand.Subscribe(() =>
        {
            bgmPlayerService.ChangeVolume(bgmPlayerService.Volume.Value - 1);
        });
    }
}
