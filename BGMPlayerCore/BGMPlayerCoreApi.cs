using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GuruGuruSmf;
using WpfAudioPlayer;
using System.IO;
using System.Collections;
using BGMPlayerCore;
using System.Reactive.Linq;
using System.ComponentModel;
using Reactive.Bindings;
using Reactive.Bindings.ObjectExtensions;
using Reactive.Bindings.Notifiers;

namespace BGMPlayerCore
{
    public class BGMPlayerCoreApi : IBGMPlayerCoreApi
    {
        #region private変数
        private IGuruGuruSmf4Api _ggs = Ggs4Dll.GetInstance();
        private AudioPlayer _audioPlayer;

        private ReactivePropertySlim<BGM> playingBGM;
        private bool isLoopableBGM = false;

        private ReactivePropertySlim<int> loopCount = default;

        private ReactivePropertySlim<PlayingState> state = default;

        private ReadOnlyReactivePropertySlim<int> midiLoopCount = default;

        private ReadOnlyReactivePropertySlim<int> audioLoopCount = default;

        private BusyNotifier loopCountUpdate = new BusyNotifier();

        private ReactiveProperty<int> volume { get; }
        #endregion

        #region　プロパティ
        private bool IsPlaying => playingBGM.Value != null;

        public ReadOnlyReactivePropertySlim<int> LoopCount { get; private set; }

        public ReadOnlyReactivePropertySlim<PlayingState> State { get; }

        public ReadOnlyReactivePropertySlim<BGM> PlayingBGM { get; }

        public ReadOnlyReactivePropertySlim<int> Volume { get; }
        #endregion

        public BGMPlayerCoreApi()
        {
            _ggs.OpenDevice(-1, (IntPtr)0);
            
            state = new ReactivePropertySlim<PlayingState>(PlayingState.Stopping);
            State = state.ToReadOnlyReactivePropertySlim();

            playingBGM = new ReactivePropertySlim<BGM>(null);
            PlayingBGM = new ReadOnlyReactivePropertySlim<BGM>(playingBGM);

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
                      if (isLoopableBGM == false) value = int.MaxValue;
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

        public async Task Play(BGM bgm)
        {
            loopCount.Value = 0;
            switch (bgm.FileExtension)
            {
                case FileExtensionType.midi:
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
                    break;
                case FileExtensionType.wave:
                case FileExtensionType.ogg:
                case FileExtensionType.mp3:
                    await _audioPlayer.Play(bgm.FullPath, bgm.FileExtension);
                    isLoopableBGM = true;
                    break;
                case FileExtensionType.other:
                default:
                    return;
            }
            playingBGM.Value = bgm;
            state.Value = PlayingState.Playing;
            ChangeVolume(volume.Value);
        }
        
        public void Stop()
        {
            if (playingBGM.Value == null) return;
            switch (playingBGM.Value.FileExtension)
            {
                case FileExtensionType.midi:
                    PlayerStatus status = _ggs.GetPlayerStatus();
                    if (status.State != PlayerState.Stop)
                    {
                        _ggs.Stop(0);
                    }
                    break;
                case FileExtensionType.wave:
                case FileExtensionType.ogg:
                case FileExtensionType.mp3:
                    _audioPlayer.Stop();
                    break;
                case FileExtensionType.other:
                default:
                    return;
            }
            playingBGM.Value = null;
            state.Value = PlayingState.Stopping;
            loopCount.Value = 0;
        }

        public void Pause()
        {
            if (!IsPlaying) return;
            switch (playingBGM.Value.FileExtension)
            {
                case FileExtensionType.midi:
                    _ggs.Pause();
                    break;
                case FileExtensionType.wave:
                case FileExtensionType.ogg:
                case FileExtensionType.mp3:
                    _audioPlayer.Pause();
                    break;
                case FileExtensionType.other:
                default:
                    break;
            }
            state.Value = PlayingState.Pausing;
        }

        public void ReStart()
        {
            if (!IsPlaying) return;
            switch (playingBGM.Value.FileExtension)
            {
                case FileExtensionType.midi:
                    _ggs.Restart();
                    break;
                case FileExtensionType.wave:
                case FileExtensionType.ogg:
                case FileExtensionType.mp3:
                    _audioPlayer.Play();
                    break;
                case FileExtensionType.other:
                default:
                    break;
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
            if (value > 10) return;
            if (value < 0) return;
            if (!IsPlaying) return;
            if (playingBGM.Value.FileExtension == FileExtensionType.midi)
            {
                if (value == 0)
                {
                    _ggs.SetMasterVolume(-127);
                }
                else
                {
                    _ggs.SetMasterVolume(-5 * (10 - value));
                }
            }
            else if (playingBGM.Value.FileExtension != FileExtensionType.other)
            {
                _audioPlayer.Volume = value * 0.1f;
            }

            this.volume.Value = value;
        }
    }
}
