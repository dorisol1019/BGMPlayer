using Reactive.Bindings;

namespace BGMList.Models
{
    public interface ISelectedBGM
    {
        ReactivePropertySlim<string> selectedBGM { get; set; }
    }
}
