using BGMPlayerCore;
using Reactive.Bindings;
using System.Collections.Generic;

namespace BGMList.Models
{
    public interface IAllBGMs
    {
        ReadOnlyReactiveProperty<List<BGM>> BGMs { get; }

        void Refresh(string path);
    }
}
