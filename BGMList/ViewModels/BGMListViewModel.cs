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

namespace BGMList.ViewModels
{
    public class BGMListViewModel : BindableBase
    {
        public ReadOnlyReactiveCollection<string> BGMs { get; }
        public ReactiveProperty<string> SelectedBGMIndex { get; set; }
        public ReactivePropertySlim<string> SelectedBGM { get; set; }

        public AsyncReactiveCommand PlayCommand { get; }

        public ReactiveCommand PauseOrRestartCommand { get; }

        private readonly IAllBGMs allBGMs;

        private readonly IBGMPlayerService bgmPlayerService;
        public BGMListViewModel(IBGMPlayerService bgmPlayerService,IAllBGMs allBGMs)
        {
            this.bgmPlayerService = bgmPlayerService;

            this.allBGMs = allBGMs;
            BGMs = allBGMs.BGMs.Select(e => e.Select(f => f.FileName).OrderBy(f => f, new CompareNaural())).SelectMany(e => e.ToObservable()).ToReadOnlyReactiveCollection();
        }
    }
}
