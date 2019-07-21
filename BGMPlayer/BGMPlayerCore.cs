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
    public class BGMPlayerCore : IBGMPlayerCore , IDisposable
    {
        #region private変数
        private IGuruGuruSmf4Api _ggs = Ggs4Dll.GetInstance();
        private AudioPlayer _audioPlayer;

        private string oldPath = "";

        private string[] _extensionNames = new[] { ".mid", ".midi", ".wav", ".wave", ".mp3", ".ogg" };
        private List<BGM> _bgmList = null;
        private BGM _selectedBGM = null;
        private bool isLoop = false;
        
        #endregion

        #region　プロパティ
        public string SelectedBGM { get => Path.GetFileName(_selectedBGM?.FileName ?? ""); }
        public List<string> BGMNameList { get => _bgmList.Select(e => Path.GetFileName(e.FileName)).ToList(); }
        public bool IsPlaying => _selectedBGM != null;
        public bool IsPause { get; private set; } = false;

        public ReadOnlyReactiveProperty<int> LoopCount { get; private set; }
        private ReactiveProperty<int> MidiLoopCount { get; set; }
        private ReadOnlyReactiveProperty<int> AudioLoopCount { get; set; }

        #endregion

        public BGMPlayerCore(IntPtr handle, string path = @"Playlist\")
        {
            _ggs.OpenDevice(-1, handle);

            this.Init(path);
        }

        public void Init(string path)
        {
            _bgmList = GetBGMList(path);

        }

        public async Task Play(int index)
        {
            var bgm = _bgmList[index];

            await Play(bgm);

            _selectedBGM = bgm;
        }

        private async Task Play(BGM bgm)
        {
            Stop();
            switch (bgm.FileExtension)
            {
                case FileExtensionType.midi:
                    _ggs.AddListFromFile(bgm.FileName, 0, 1);
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
                    MidiLoopCount = _ggs.ObserveEveryValueChanged(e => e.GetPlayerStatus().LoopCount).ToReactiveProperty(mode:ReactivePropertyMode.DistinctUntilChanged);
                    MidiLoopCount.Subscribe((i)=>
                    {
                        if (isLoop == false) MidiLoopCount.Value = int.MaxValue;
                    }
                    );
                    LoopCount = MidiLoopCount.ToReadOnlyReactiveProperty();
                    break;
                case FileExtensionType.wave:
                case FileExtensionType.ogg:
                case FileExtensionType.mp3:
                    if (_audioPlayer != null) _audioPlayer.Dispose();
                    _audioPlayer = new AudioPlayer();
                    await _audioPlayer.Play(bgm.FileName, bgm.FileExtension);
                    isLoop = true;
                    AudioLoopCount = _audioPlayer.ObserveEveryValueChanged(e => e.LoopCount).ToReadOnlyReactiveProperty();
                    LoopCount = AudioLoopCount;
                    break;
                case FileExtensionType.other:
                default:
                    return;
            }
            IsPause = false;
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
                    _audioPlayer.Dispose();
                    _audioPlayer = null;
                    break;
                case FileExtensionType.other:
                default:
                    return;
            }
            _selectedBGM = null;
            IsPause = false;
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
        }

        public void Dispose()
        {
            Stop();
            _ggs.CloseDevice();
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
        private List<BGM> GetBGMList(string path)
        {
            if (oldPath == path) return _bgmList;
            oldPath = path;

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
        #endregion
    }
}
