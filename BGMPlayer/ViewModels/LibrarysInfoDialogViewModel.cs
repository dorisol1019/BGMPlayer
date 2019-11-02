using BGMPlayer.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BGMPlayer.ViewModels
{
    public class LibrarysInfoDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "ライブラリの情報";

        public string Text { get; set; } = "";

        private LibrarysInfo librarysInfo = new LibrarysInfo();

        public event Action<IDialogResult> RequestClose = (_) => { };

        public LibrarysInfoDialogViewModel()
        {
            Text = librarysInfo.Text;
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}
