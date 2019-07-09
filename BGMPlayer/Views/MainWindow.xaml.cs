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
        public MainWindow()
        {
            InitializeComponent();
           
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
        
        
        
    }
}
