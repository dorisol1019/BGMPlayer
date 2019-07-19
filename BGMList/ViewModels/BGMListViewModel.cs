using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BGMList.ViewModels
{
    public class BGMListViewModel : BindableBase
    {
        public ReactiveCollection<string> BGMs { get; }
        public ReactiveProperty<string> SelectedBGMIndex { get; set; }
        public ReactivePropertySlim<string> SelectedBGM { get; set; }
        public BGMListViewModel()
        {

        }
    }
}
