using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Mvvm;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.Windows.Input;
using Prism.Commands;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using Reactive.Bindings.Notifiers;
using Reactive.Bindings.Extensions;
using System.Windows.Threading;
using Prism.Interactivity.InteractionRequest;
using System.IO;
using BGMPlayerCore;
using System.Collections;
using BGMList.Models;

namespace BGMPlayer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        public ReactiveProperty<string> Title { get; }

        const string _defaultTitle = "BGM鳴ら～すV3";
        

        public ICommand Shutdown { get; }
        public ICommand OpenFolderCommand { get; }

        public ICommand WindowClosedCommand { get; }

        private IAllBGMs allBGMs;

        public MainWindowViewModel(IBGMPlayerService bgmPlayerService, IAllBGMs allBGMs)
        {
            Title = new ReactiveProperty<string>(_defaultTitle);

            Shutdown = new DelegateCommand(() => Application.Current.Shutdown());

            this.allBGMs = allBGMs;

            bgmPlayerService.State.Subscribe(state => {
                switch (state)
                {
                    case PlayingState.Playing:
                        this.Title.Value = $"再生中 : {bgmPlayerService.PlayingBGM.Value.FileName}";
                        break;
                    case PlayingState.Stopping:
                        this.Title.Value = _defaultTitle;
                        break;
                    case PlayingState.Pausing:
                        this.Title.Value = $"一時停止 : {bgmPlayerService.PlayingBGM.Value.FileName}";
                        break;
                    default:
                        break;
                }
            });

            _interactionRequest = new InteractionRequest<INotification>();
            PopUpVersionInfoCommand = new DelegateCommand(() =>
              _interactionRequest.Raise(new Notification { Title = "BGM鳴ら～すV3について" })
            );

            OpenFolderCommand = new DelegateCommand(() => OpenFolder());

            WindowClosedCommand = new DelegateCommand(() => bgmPlayerService.Dispose());
        }

        private InteractionRequest<INotification> _interactionRequest;
        public DelegateCommand PopUpVersionInfoCommand { get; }

        public InteractionRequest<INotification> InteractionRequest
        {
            get => _interactionRequest;
        }

        void OpenFolder()
        {
            var dig = new Models.OpenFolderDialog();
            dig.Show();

            var folderName = dig.FolderName;

            if (folderName == "") return;

            this.allBGMs.Refresh(folderName);
        }
    }
}
