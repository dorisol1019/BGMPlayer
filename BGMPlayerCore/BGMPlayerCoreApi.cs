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

        private BGM _selectedBGM = null;
        private bool isLoopableBGM = false;

        private ReactivePropertySlim<int> loopCount = default;

        private ReactivePropertySlim<PlayingState> state = default;

        private ReadOnlyReactivePropertySlim<int> midiLoopCount = default;

        private ReadOnlyReactivePropertySlim<int> audioLoopCount = default;

        private BusyNotifier loopCountUpdate = new BusyNotifier();
        #endregion

        #region　プロパティ
        public bool IsPlaying => _selectedBGM != null;
        public bool IsPause { get; private set; } = false;

        public ReadOnlyReactivePropertySlim<int> LoopCount { get; private set; }

        public ReadOnlyReactivePropertySlim<PlayingState> State { get; }
        #endregion

        public BGMPlayerCoreApi()
        {
            _ggs.OpenDevice(-1, (IntPtr)0);

            state = new ReactivePropertySlim<PlayingState>(PlayingState.Stopping);
            State = state.ToReadOnlyReactivePropertySlim();

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
            IsPause = false;
            state.Value = PlayingState.Playing;
            _selectedBGM = bgm;
        }
        
        public void Stop()
        {
            if (_selectedBGM == null) return;
            switch (_selectedBGM.FileExtension)
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
            _selectedBGM = null;
            IsPause = false;
            state.Value = PlayingState.Stopping;
            loopCount.Value = 0;
        }

        public void Pause()
        {
            if (!IsPlaying) return;
            switch (_selectedBGM.FileExtension)
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
            IsPause = true;
            state.Value = PlayingState.Pausing;
        }

        public void ReStart()
        {
            if (!IsPlaying) return;
            switch (_selectedBGM.FileExtension)
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
            IsPause = false;
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
            if (!IsPlaying) return;
            if (_selectedBGM.FileExtension == FileExtensionType.midi)
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
            else if (_selectedBGM.FileExtension != FileExtensionType.other)
            {
                _audioPlayer.Volume = value * 0.1f;
            }
        }
    }
}
