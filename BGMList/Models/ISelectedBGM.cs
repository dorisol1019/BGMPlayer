using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGMList.Models
{
    public interface ISelectedBGM
    {
        ReactivePropertySlim<string> selectedBGM { get; set; }
    }
}
