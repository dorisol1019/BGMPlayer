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

namespace BGMPlayer
{
    public delegate void LoopEventHandler(int loopCount);

    public class BGMPlayerCore : IDisposable,INotifyPropertyChanged
    {
        #region private変数
        private IGuruGuruSmf4Api _ggs = Ggs4Dll.GetInstance();
        private AudioPlayer _audioPlayer;

        private string oldPath = "";

        private string[] _extensionNames = new[] { ".mid", ".midi", ".wav", ".wave", ".mp3", ".ogg" };
        private List<BGM> _bgmList = null;
        private BGM _selectedBGM = null;
        private bool isLoop=false;
        
        private IDisposable loopTimerDisposer;
        #endregion

        #region　プロパティ
        public string SelectedBGM { get => Path.GetFileName(_selectedBGM?.FileName ?? ""); }
        public List<string> BGMNameList { get => _bgmList.Select(e => Path.GetFileName(e.FileName)).ToList(); }
        public bool IsPlaying => _selectedBGM != null;
        public bool IsPause { get; private set; } = false;
        private int noLoopBGM_Loopcount = int.MaxValue;
        private int oldLoopCount = 0;
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
            switch (bgm.FileExtension)
            {
                case FileExtensionType.midi:
                    _ggs.AddListFromFile(bgm.FileName, 0, 1);
                    _ggs.Play(PlayOption.Loop | PlayOption.SkipBeginning, 1, 0, 0, 0);
                    _ggs.GetSmfInformation(out SmfInformation info, 1);
                    if (info.LoopTime >= info.LastNoteTime || info.LoopTick == -1)
                    {
                        if (_selectedBGM != bgm) noLoopBGM_Loopcount = 0;
                        
                        isLoop = false;

                    }
                    else
                    {
                        noLoopBGM_Loopcount = int.MaxValue;
                        isLoop = true;
                    }
                    break;
                case FileExtensionType.wave:
                case FileExtensionType.ogg:
                case FileExtensionType.mp3:
                    if (_audioPlayer != null) _audioPlayer.Dispose();
                    _audioPlayer = new AudioPlayer();
                    await _audioPlayer.Play(bgm.FileName, bgm.FileExtension);
                    isLoop = true;
                    break;
                case FileExtensionType.other:
                default:
                    return;
            }
            oldLoopCount = 0;
            IsPause = false;
            var timer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(30));
            loopTimerDisposer = timer.Subscribe( x =>
            {
                if (!IsPlaying) return;
                int loopCount = 0;
                switch (_selectedBGM.FileExtension)
                {
                    case FileExtensionType.midi:
                        PlayerStatus status = _ggs.GetPlayerStatus();
                        loopCount = status.LoopCount;
                        if (!isLoop)
                        {
                            if (status.State == PlayerState.Stop)
                            {
                                noLoopBGM_Loopcount++;
                                var selectedname = _selectedBGM.FileName;
                                var selectedex = _selectedBGM.FileExtension;
                                //Stop();
                                //await Play(new BGM(selectedname, selectedex));// _selectedBGM);
                                _ggs.Play(PlayOption.SkipBeginning, 1, 0, 0, 0);

                                loopCount = noLoopBGM_Loopcount;
                            }
                        }
                        else { noLoopBGM_Loopcount = int.MaxValue; }
                        break;
                    case FileExtensionType.wave:
                    case FileExtensionType.ogg:
                    case FileExtensionType.mp3:
                        loopCount = _audioPlayer?.LoopCount ?? 0;
                        break;
                    case FileExtensionType.other:
                        break;
                    default:
                        break;
                }
                if (loopCount > oldLoopCount)
                {
                    oldLoopCount = loopCount;
                    //LoopEvent(loopCount);
                }
                LoopCount = loopCount;
            });
        }
        private int loopCount = 0;
        public int LoopCount {
            get =>loopCount;
            private set
            {
                if (loopCount == value) return;
                loopCount = value;
                PropertyChanged?.Invoke(this, LoopCountPropertyChangedEventArgs);
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly PropertyChangedEventArgs LoopCountPropertyChangedEventArgs
            = new PropertyChangedEventArgs(nameof(LoopCount));

        public void Stop()
        {
            if (_selectedBGM == null) return;
            switch (_selectedBGM.FileExtension)
            {
                case FileExtensionType.midi:
                    _ggs.Stop(0);
                    _ggs.DeleteListItem(1);
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
            oldLoopCount = 0;
            loopTimerDisposer.Dispose();
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
