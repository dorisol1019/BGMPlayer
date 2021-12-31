using GuruGuruSmf;
using Reactive.Bindings;
using Reactive.Bindings.Notifiers;
using Reactive.Bindings.ObjectExtensions;
using System.Reactive.Linq;
using WpfAudioPlayer;

namespace BGMPlayerCore;

public class BGMPlayerCoreApi : IBGMPlayerCoreApi
{
    #region private変数
    private readonly IGuruGuruSmf4Api _ggs = Ggs4Dll.GetInstance();
    private readonly AudioPlayer _audioPlayer;

    private readonly ReactivePropertySlim<BgmFilePath> playingBGM;
    private bool isLoopableBGM = false;

    private readonly ReactivePropertySlim<int>? loopCount = default;

    private readonly ReactivePropertySlim<PlayingState>? state = default;

    private readonly ReadOnlyReactivePropertySlim<int>? midiLoopCount = default;

    private readonly ReadOnlyReactivePropertySlim<int>? audioLoopCount = default;

    private readonly BusyNotifier loopCountUpdate = new BusyNotifier();

    private ReactiveProperty<int> volume { get; }
    #endregion

    #region　プロパティ
    private bool IsPlaying => playingBGM.Value != null;

    public ReadOnlyReactivePropertySlim<int> LoopCount { get; private set; }

    public ReadOnlyReactivePropertySlim<PlayingState> State { get; }

    public ReadOnlyReactivePropertySlim<BgmFilePath> PlayingBGM { get; }

    public ReadOnlyReactivePropertySlim<int> Volume { get; }
    #endregion

    public BGMPlayerCoreApi()
    {
        _ggs.OpenDevice(-1, (IntPtr)0);

        state = new ReactivePropertySlim<PlayingState>(PlayingState.Stopping);
        State = state.ToReadOnlyReactivePropertySlim();

        playingBGM = new ReactivePropertySlim<BgmFilePath>(null);
        PlayingBGM = new ReadOnlyReactivePropertySlim<BgmFilePath>(playingBGM);

        _audioPlayer = new AudioPlayer();

        loopCount = new ReactivePropertySlim<int>(0);
        LoopCount = loopCount.ToReadOnlyReactivePropertySlim();

        midiLoopCount = _ggs.ObserveEveryValueChanged(e => e.GetPlayerStatus().LoopCount).ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.None);

        audioLoopCount = _audioPlayer.ObserveEveryValueChanged(
            e =>
            e.LoopCount
            ).ToReadOnlyReactivePropertySlim(mode: ReactivePropertyMode.None);
        midiLoopCount.Where(_ => !loopCountUpdate.IsBusy).Subscribe(count =>
          {
              using (loopCountUpdate.ProcessStart())
              {
                  int value = count;
                  if (isLoopableBGM == false)
                  {
                      value = int.MaxValue;
                  }

                  loopCount.Value = value;
              }
          });
        audioLoopCount.Where(_ => !loopCountUpdate.IsBusy).Subscribe(count =>
          {
              using (loopCountUpdate.ProcessStart())
              {
                  loopCount.Value = count;
              }
          });

        volume = new ReactiveProperty<int>(5);
        Volume = new ReadOnlyReactivePropertySlim<int>(volume);
    }

    public async Task Play(BgmFilePath bgm)
    {
        loopCount.Value = 0;

        if (bgm.BgmType == BgmType.Midi)
        {
            _ggs.AddListFromFile(bgm.FullPath, 0, 1);
            _ggs.Play(PlayOption.Loop, 1, 0, 0, 0);
            _ggs.GetSmfInformation(out SmfInformation info, 1);
            if (info.LoopTime >= info.LastNoteTime)
            {
                isLoopableBGM = false;
            }
            else
            {
                isLoopableBGM = true;
            }
        }
        else
        {
            await _audioPlayer.Play(bgm.FullPath, bgm.BgmType);
            isLoopableBGM = true;
        }
        playingBGM.Value = bgm;
        state.Value = PlayingState.Playing;
        ChangeVolume(volume.Value);
    }

    public void Stop()
    {
        if (playingBGM.Value == null)
        {
            return;
        }

        if (playingBGM.Value.BgmType == BgmType.Midi)
        {
            PlayerStatus status = _ggs.GetPlayerStatus();
            if (status.State != PlayerState.Stop)
            {
                _ggs.Pause();
            }
        }
        else
        {
            _audioPlayer.Stop();

        }

        playingBGM.Value = null;
        state.Value = PlayingState.Stopping;
        loopCount.Value = 0;
    }

    public void Pause()
    {
        if (!IsPlaying)
        {
            return;
        }
        if (playingBGM.Value.BgmType == BgmType.Midi)
        {
            _ggs.Pause();
        }
        else
        {
            _audioPlayer.Pause();
        }
        state.Value = PlayingState.Pausing;
    }

    public void ReStart()
    {
        if (!IsPlaying)
        {
            return;
        }

        if (playingBGM.Value.BgmType == BgmType.Midi)
        {
            _ggs.Restart();
        }
        else
        {
            _audioPlayer.Play();
        }

        state.Value = PlayingState.Playing;
    }

    public void Dispose()
    {
        Stop();
        _ggs.CloseDevice();
        _audioPlayer.Dispose();
    }

    public void ChangeVolume(int value)
    {
        if (value > 10)
        {
            return;
        }

        if (value < 0)
        {
            return;
        }

        if (!IsPlaying)
        {
            return;
        }

        if (value == 0)
        {
            _ggs.SetMasterVolume(-127);
        }
        else
        {
            _ggs.SetMasterVolume(-5 * (10 - value));
        }
        _audioPlayer.Volume = value * 0.1f;

        volume.Value = value;
    }
}
