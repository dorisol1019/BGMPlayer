using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;

namespace BGMList.Models
{
    public class SelectedBGM : ISelectedBGM
    {
        public ReactivePropertySlim<string> selectedBGM { get; set; }
    }
}
