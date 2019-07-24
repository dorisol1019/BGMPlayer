using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GuruGuruSmf;
using WpfAudioPlayer;
using System.IO;
using System.Collections;
using BGMPlayer.Extension;
using System.Reactive.Linq;
using System.ComponentModel;
using Reactive.Bindings;
using Reactive.Bindings.ObjectExtensions;

namespace BGMPlayer
{
    public class BGMPlayerCore : IBGMPlayerCore
    {
        #region private変数
        private IGuruGuruSmf4Api _ggs = Ggs4Dll.GetInstance();
        private AudioPlayer _audioPlayer;

        private BGM _selectedBGM = null;
        private bool isLoop = false;

        private ReactivePropertySlim<int> loopCount = default;

        private IDisposable loopCountUnSubscriber = default;

        #endregion

        #region　プロパティ
        public string SelectedBGM { get => _selectedBGM?.FileName ?? ""; }
        public bool IsPlaying => _selectedBGM != null;
        public bool IsPause { get; private set; } = false;

        public ReadOnlyReactiveProperty<int> LoopCount { get; private set; }
        private ReactiveProperty<int> MidiLoopCount { get; set; }
        private ReadOnlyReactiveProperty<int> AudioLoopCount { get; set; }

        public ReactivePropertySlim<PlayingState> State { get; }
        #endregion

        public BGMPlayerCore()
        {
            _ggs.OpenDevice(-1, (IntPtr)0);

            State = new ReactivePropertySlim<PlayingState>(PlayingState.Stopping);

            _audioPlayer = new AudioPlayer();

            loopCount = new ReactivePropertySlim<int>(0);
            LoopCount = loopCount.ToReadOnlyReactiveProperty();

            MidiLoopCount = _ggs.ObserveEveryValueChanged(e => e.GetPlayerStatus().LoopCount).ToReactiveProperty(mode: ReactivePropertyMode.DistinctUntilChanged);
            AudioLoopCount = _audioPlayer.ObserveEveryValueChanged(e => e.LoopCount).ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.DistinctUntilChanged);

        }

        public async Task Play(BGM bgm)
        {
            switch (bgm.FileExtension)
            {
                case FileExtensionType.midi:
                    _ggs.AddListFromFile(bgm.FullPath, 0, 1);
                    _ggs.Play(PlayOption.Loop, 1, 0, 0, 0);
                    _ggs.GetSmfInformation(out SmfInformation info, 1);
                    if (info.LoopTime >= info.LastNoteTime)
                    {
                        isLoop = false;
                    }
                    else
                    {
                        isLoop = true;
                    }
                    loopCountUnSubscriber = MidiLoopCount.Subscribe((i)=>
                    {
                        if (isLoop == false) MidiLoopCount.Value = int.MaxValue;
                        loopCount.Value = MidiLoopCount.Value;
                    }
                    );
                    break;
                case FileExtensionType.wave:
                case FileExtensionType.ogg:
                case FileExtensionType.mp3:
                    await _audioPlayer.Play(bgm.FullPath, bgm.FileExtension);
                    isLoop = true;
                    loopCountUnSubscriber = AudioLoopCount.Subscribe((count) =>
                    {
                        loopCount.Value = count;
                    });
                    break;
                case FileExtensionType.other:
                default:
                    return;
            }
            IsPause = false;
            State.Value = PlayingState.Playing;
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
            State.Value = PlayingState.Stopping;
            loopCountUnSubscriber?.Dispose();
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
            State.Value = PlayingState.Pausing;
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
            State.Value = PlayingState.Playing;
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



        #region

        #endregion
    }
}
