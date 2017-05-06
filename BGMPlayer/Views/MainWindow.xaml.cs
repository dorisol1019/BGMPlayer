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

namespace BGMPlayer.Views
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
            path = @"Playlist\";
#endif

            player = new BGMPlayerCore(new WindowInteropHelper(this).Handle, path);



            player.LoopEvent += Player_LoopEvent;
            
            this.Loaded += MainWindow_Loaded;
            

            loopOption.SelectionChanged += LoopOption_SelectionChanged;
            loopNumber.PreviewTextInput += LoopNumber_PreviewTextInput;
            

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
            var dig = new VersionInfoDialog()
            {
                Owner = this
            };
            dig.Show();
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
        

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChangeVolume();
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
        


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            loopOption.SelectedIndex = 1;
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
        
        #endregion




        const string defaultTitle = "BGM鳴ら～すV3";
        private int selectedBGMIndex = -1;
    }
}
