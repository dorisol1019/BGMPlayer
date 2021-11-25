using BGMPlayerCore;
using Reactive.Bindings;

namespace BGMList.Models;

public interface IAllBGMs
{
    ReadOnlyReactiveProperty<List<BgmFilePath>> BGMs { get; }

    void Refresh(string path);
}
