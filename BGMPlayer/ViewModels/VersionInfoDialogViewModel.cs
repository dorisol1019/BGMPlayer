using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Commands;
using Prism.Mvvm;

using Reactive.Bindings;

using BGMPlayer.Models;
using System.Windows.Input;
using Prism.Interactivity.InteractionRequest;
using Prism.Services.Dialogs;

namespace BGMPlayer.ViewModels
{
    public class VersionInfoDialogViewModel : BindableBase, IDialogAware
    {
        VersionInfo _versionInfo = new VersionInfo();

        public event Action<IDialogResult> RequestClose = (_) => { };

        public ReactiveProperty<string> ApplicationName { get; private set; }
        public ReactiveProperty<string> ApplicationVersion { get; }

        public ReactiveProperty<string> Copyright { get; }
        public ReactiveProperty<string> ProjectURL { get; }

        public ICommand NavigateToProjectURL { get; }

        public string Title => "BGM鳴ら～すV3について";

        public VersionInfoDialogViewModel()
        {
            ApplicationName = new ReactiveProperty<string>(_versionInfo.ApplicationName);
            ApplicationVersion = new ReactiveProperty<string>(_versionInfo.ApplicationVersion);

            Copyright = new ReactiveProperty<string>(_versionInfo.CopyrightText);
            ProjectURL = new ReactiveProperty<string>(_versionInfo.ProjectURL);

            NavigateToProjectURL = new DelegateCommand(_versionInfo.NavigateToProjectURL);
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
