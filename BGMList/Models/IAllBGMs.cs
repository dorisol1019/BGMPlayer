using BGMPlayerCore;
using Reactive.Bindings;
using System.Collections.Generic;

namespace BGMList.Models
{
    public interface IAllBGMs
    {
        ReadOnlyReactiveProperty<List<BgmFilePath>> BGMs { get; }

        void Refresh(string path);
    }
}
