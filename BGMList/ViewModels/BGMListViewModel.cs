using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using BGMList.Models;
using System.Reactive.Linq;
using BGMPlayer;
using BGMPlayerService;
using BGMPlayerCore;

namespace BGMList.ViewModels
{
    public class BGMListViewModel : BindableBase
    {
        public ReadOnlyReactiveProperty<List<string>> BGMs { get; }
        public ReactiveProperty<int> SelectedBGMIndex { get; set; }
        public ReactivePropertySlim<string> SelectedBGM { get; set; }

        public AsyncReactiveCommand PlayCommand { get; }

        public ReactiveCommand PauseOrRestartCommand { get; }

        private readonly IAllBGMs allBGMs;

        private readonly IBGMPlayerService bgmPlayerService;
        public BGMListViewModel(IBGMPlayerService bgmPlayerService, IAllBGMs allBGMs, ISelectedBGM selectedBGM, IUserOperationNotification<BGM> playingBGMNotification)
        {
            this.bgmPlayerService = bgmPlayerService;

            this.allBGMs = allBGMs;
            BGMs = allBGMs.BGMs.Select(e => e.Select(f => f.FileName).OrderBy(f => f, new CompareNaural()).ToList()).ToReadOnlyReactiveProperty();

            SelectedBGM = selectedBGM.selectedBGM;
            SelectedBGMIndex = new ReactiveProperty<int>(0);

            PlayCommand = new AsyncReactiveCommand();
            PlayCommand.Subscribe(() =>
            {
                var bgm = allBGMs.BGMs.Value.First(e => e.FileName == SelectedBGM.Value);
                playingBGMNotification.Notification.Value = bgm;
                return bgmPlayerService.Play(bgm);
            });

            PauseOrRestartCommand = new ReactiveCommand();
            PauseOrRestartCommand.Subscribe(bgmPlayerService.PauseOrReStart);
        }
    }
}
