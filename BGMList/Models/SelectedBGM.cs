using Reactive.Bindings;

namespace BGMList.Models;

public class SelectedBGM : ISelectedBGM
{
    public ReactivePropertySlim<string> selectedBGM { get; set; } = new ReactivePropertySlim<string>("");
}
