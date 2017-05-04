using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BGMPlayer
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private BGMPlayerCore player;
        public MainWindow()
        {
            InitializeComponent();

            string path = @"Playlist\";
#if DEBUG
            //path = @"testplaylist";
            //path = @"Playlist\";
#endif

            player = new BGMPlayerCore(new WindowInteropHelper(this).Handle, path);



            player.LoopEvent += Player_LoopEvent;

            this.Title = defaultTitle;
            this.Closed += MainWindow_Closed;
            this.Loaded += MainWindow_Loaded;
            ResizeMode = ResizeMode.NoResize;

            //            BGMList.FontFamily = new FontFamily("MeiryoKe_Gothic");//, 10.5f, GraphicsUnit.Point);
            BGMList.FontSize = (double)(new FontSizeConverter()).ConvertFromString("10pt");
            var bgmnamelist = player.BGMNameList;
            if (bgmnamelist.Count == 0)
            {
                MessageBox.Show("Playlistフォルダに音楽ファイルがありません\n" +
                    "音楽ファイルをPlaylistフォルダに入れてから起動して下さい", "Error!!");
                BGMList.ItemsSource = new List<string>[0];
            }
            else
            {
                BGMList.ItemsSource = bgmnamelist;
            }

            BGMList.SelectedIndex = 0;
            BGMList.SelectionMode = SelectionMode.Single;
            BGMList.KeyUp += BGMList_KeyUp;
            BGMList.KeyDown += BGMList_KeyDown;


            PlayButton.Click += PlayButton_Click;
            StopButton.Click += StopButton_Click;
            PauseOrRestartButton.Click += PauseOrRestartButton_Click;

            volumeSlider.Value = 5;
            volumeSlider.ValueChanged += VolumeSlider_ValueChanged;

            IsTopMostWindow.Checked += IsTopMostWindow_Checked;
            IsTopMostWindow.Unchecked += IsTopMostWindow_Checked;

            loopOption.SelectionChanged += LoopOption_SelectionChanged;
            loopNumber.PreviewTextInput += LoopNumber_PreviewTextInput;

            //OpenFolderMenu.Click += OpenFolderMenu_Click;
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OpenFolderMenu_Click));
            ReStartMenu.Click += ReStartMenu_Click;
            endMenu.Click += EndMenu_Click;

            VersionInfoMenu.Click += VersionInfoMenu_Click;
            LibMenu.Click += LibMenu_Click;
        }

        private void LibMenu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "GuruGuruSMF Copyright (c) 卯如 \n"+
                "http://gurugurusmf.migmig.net/ \n\n" +
                "" +
                "libogg Copyright (c) 2002, Xiph.org Foundation.\n" +
                "libvorbis Copyright(c) 2002-2015 Xiph.org Foundation.\n" +
                "OggLoader Copyright (c) 2009 あひるわーくす\n" +
                "https://www.xiph.org/licenses/bsd/　" +
                "http://www.ahiruworks.com/index.html　\n\n" +

                "NAudio Copyright (c) 2017 Mark Heath \n" +
                "https://naudio.codeplex.com/license \n\n" +
                "" +
                "WindowsAPICodePack Copyright (c)  Microsoft \n" +
                "http://web.archive.org/web/20120126200915/http://archive.msdn.microsoft.com/WindowsAPICodePack/Project/License.aspx \n\n" +
                "" +
                "Reactive Extensions for .NET Copyright(c).NET Foundation and Contributors \n" +
                "https://github.com/Reactive-Extensions/Rx.NET/blob/develop/LICENSE"
                , "ライブラリの情報");
        }

        private void VersionInfoMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Reflection.Assembly asm =
                System.Reflection.Assembly.GetExecutingAssembly();
            //バージョンの取得
            System.Version ver = asm.GetName().Version;
            MessageBox.Show($"BGM鳴ら～すV3 {ver.ToString()}\n" +
                $"Copyright (c) 2017 dorifru0209",
                "BGMPlayerV3 About");
        }

        private void OpenFolderMenu_Click(object sender, RoutedEventArgs e)
        {
            var dig = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog()
            {
                Title = "フォルダを開く",
                InitialDirectory = System.Environment.CurrentDirectory,
                IsFolderPicker = true,
                AddToMostRecentlyUsedList = false,
                Multiselect = false,
                ShowPlacesList = true,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureValidNames = true,
                EnsureReadOnly = false,
                DefaultDirectory = System.Environment.CurrentDirectory
            };
            if (dig.ShowDialog(this) == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                var folderName = dig.FileName;
                player.Init(folderName);
                BGMList.ItemsSource = player.BGMNameList;
                BGMList.SelectedIndex = 0;
                selectedBGMIndex = -1;
            }
        }

        private void ReStartMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(App.ResourceAssembly.Location, "/restart");
            Application.Current.Shutdown();
        }

        private void EndMenu_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void LoopOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loopOption.SelectedIndex == 1)
            {
                loopLabel.Visibility = Visibility.Visible;
                loopNumber.Visibility = Visibility.Visible;
                LoopOption_Next.Visibility = Visibility.Visible;
                LoopOption_Shuffle.Visibility = Visibility.Visible;
            }
            else
            {
                loopLabel.Visibility = Visibility.Hidden;
                loopNumber.Visibility = Visibility.Hidden;
                LoopOption_Next.Visibility = Visibility.Hidden;
                LoopOption_Shuffle.Visibility = Visibility.Hidden;
            }
        }



        private void LoopNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            bool canParse = false;
            {
                var tmp = loopNumber.Text + e.Text;
                canParse = uint.TryParse(tmp, out uint x);
            }

            e.Handled = !canParse;
        }

        private async void Player_LoopEvent(int loopCount)
        {
            await Dispatcher.Invoke(async() =>
            {
                int index = -1;
                if (loopOption.SelectedIndex == 0) return;
                string txt = loopNumber.Text;
                int loopN = 0;
                if (string.IsNullOrEmpty(txt)) loopN = 0;
                else loopN = int.Parse(txt);
                if (loopN + 1 > loopCount) return;

                if (LoopOption_Shuffle.IsChecked.Value)
                {
                    var rand = new Random();

                    index = rand.Next(BGMList.Items.Count);
                }
                else if (LoopOption_Next.IsChecked.Value)
                {
                    index = selectedBGMIndex + 1;
                    if (BGMList.Items.Count <= index) index = 0;
                }
                Stop();
                await Play(index);
                BGMList.SelectedIndex = selectedBGMIndex;
            });
        }

        private void IsTopMostWindow_Checked(object sender, RoutedEventArgs e)
        {
            this.Topmost = !Topmost;
        }
        static bool doubleClicked = false;
        private async void BGMList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (doubleClicked) return;
            doubleClicked = true;
            await Play();
            doubleClicked = false;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeVolume();
        }

        private void BGMList_KeyDown(object sender, KeyEventArgs e)
        {
            var modifiers = Keyboard.Modifiers;
            if ((modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                if (e.Key == Key.Left)
                {
                    if (volumeSlider.Value > 0)
                    {
                        volumeSlider.Value -= 1;
                        ChangeVolume();
                    }
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    if (volumeSlider.Value < 10)
                    {
                        volumeSlider.Value += 1;
                        ChangeVolume();
                    }
                    e.Handled = true;
                }
            }
        }

        private void PauseOrRestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (player.IsPlaying)
            {
                if (!player.IsPause)
                {
                    Pause();
                }
                else
                {
                    Restart();
                }
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private async void BGMList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await  Play();
            }
            if (e.Key == Key.Space)
            {
                if (player.IsPause)
                {
                    Restart();
                }
                else
                {
                    Pause();
                }
            }
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            loopOption.SelectedIndex = 1;
            Keyboard.Focus(BGMList);
        }

        static bool isPlayButtonClick = false;
        private async  void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (BGMList.Items.Count == 0) return;
            if (isPlayButtonClick) return;
            isPlayButtonClick = true;
            await Play();
            isPlayButtonClick = false;
        }


        private async Task Play()
        {
            if (BGMList.SelectedItem as string == player.SelectedBGM)
            {
                if (player.IsPause)
                {
                    Restart();
                }
                else
                {
                    Pause();
                }
            }
            Stop();

            int index = BGMList.SelectedIndex;

            await Play(index);
        }

        private async Task Play(int index)
        {
            await player.Play(index);

            ChangeVolume();
            PauseOrRestartButton.Content = "一時停止";
            this.Title = $"再生中:{player.SelectedBGM}";
            selectedBGMIndex = index;
        }

        private void Stop()
        {
            player.Stop();
            PauseOrRestartButton.Content = "";
            this.Title = defaultTitle;
        }

        private void Pause()
        {
            player.Pause();
            PauseOrRestartButton.Content = "停止解除";
            this.Title = $"一時停止中:{player.SelectedBGM}";
        }

        private void Restart()
        {
            player.ReStart();
            PauseOrRestartButton.Content = "一時停止";
            this.Title = $"再生中:{player.SelectedBGM}";
        }

        private void ChangeVolume()
        {
            player.ChangeVolume((int)volumeSlider.Value);
        }



        #region イベントハンドラ
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            player.Dispose();
        }
        #endregion




        const string defaultTitle = "BGM鳴ら～すV3";
        private int selectedBGMIndex = -1;
    }
}
