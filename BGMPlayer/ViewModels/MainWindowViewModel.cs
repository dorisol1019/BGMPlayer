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

namespace BGMPlayer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {

        public ReactiveProperty<string> Title { get; }

        const string _defaultTitle = "BGM鳴ら～すV3";
        

        public ICommand Shutdown { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand RestartCommand { get; }
        public ICommand CtrlRightCommand { get; }
        public ICommand CtrlLeftCommand { get; }
        public ICommand SpaceCommand { get; }
        public ICommand EnterCommand { get; }
        public ICommand MouseDoubleClickCommand { get; }
        public ICommand WindowClosedCommand { get; }


        public ReactiveProperty<string> LoopNumber_string { get; }
        public ReadOnlyReactiveProperty<int> LoopNumber { get; }
        public ReactiveProperty<TextCompositionEventArgs> LoopNumber_PreviewTextInput { get; }
        public ReactiveProperty<int> LoopOptionSelectedIndex { get; }
        public ReactiveProperty<Visibility> LoopShuffleVisibility { get; }

        public ReactiveProperty<bool> IsShuffleChecked { get; }
        public ReactiveProperty<bool> IsNextChecked { get; }

        IBGMPlayerService player;
        public MainWindowViewModel(IBGMPlayerService player)
        {
            Title = new ReactiveProperty<string>(_defaultTitle);

            this.player = player;


            Shutdown = new DelegateCommand(() => Application.Current.Shutdown());



            WindowClosedCommand = new DelegateCommand(() => player.Dispose());

            LoopNumber_string = new ReactiveProperty<string>("0", mode: ReactivePropertyMode.None);
            LoopNumber = LoopNumber_string.Select(e => int.TryParse(e, out int result) ? result : 0).ToReadOnlyReactiveProperty();

            LoopNumber_PreviewTextInput = new ReactiveProperty<TextCompositionEventArgs>(mode: ReactivePropertyMode.None);
            LoopNumber_PreviewTextInput.Subscribe((e) =>
            {
                bool canParse = false;
                {
                    var tmp = LoopNumber_string.Value + e.Text;
                    canParse = uint.TryParse(tmp, out uint x);
                }
                e.Handled = !canParse;
            });
            LoopShuffleVisibility = new ReactiveProperty<Visibility>();
            LoopOptionSelectedIndex = new ReactiveProperty<int>(1);
            LoopOptionSelectedIndex.Subscribe(_ =>
            {
                if (LoopOptionSelectedIndex.Value == 1)
                {
                    LoopShuffleVisibility.Value = Visibility.Visible;
                }
                else
                {
                    LoopShuffleVisibility.Value = Visibility.Hidden;
                }
            });

            IsShuffleChecked = new ReactiveProperty<bool>(true);
            IsNextChecked = IsShuffleChecked.Inverse().ToReactiveProperty();


            _interactionRequest = new InteractionRequest<INotification>();
            PopUpVersionInfoCommand = new DelegateCommand(() =>
              _interactionRequest.Raise(new Notification { Title = "BGM鳴ら～すV3について" })
            );

        }

        private InteractionRequest<INotification> _interactionRequest;
        public DelegateCommand PopUpVersionInfoCommand { get; }

        public InteractionRequest<INotification> InteractionRequest
        {
            get => _interactionRequest;
        }
    }
}
