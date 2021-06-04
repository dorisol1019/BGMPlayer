using Reactive.Bindings;
using System;
using System.Threading.Tasks;

namespace BGMPlayerCore
{
    public interface IBGMPlayerCoreApi : IDisposable
    {
        Task Play(BGM bgm);

        void Stop();

        void Pause();

        void ReStart();

        void ChangeVolume(int volume);

        ReadOnlyReactivePropertySlim<PlayingState> State { get; }

        ReadOnlyReactivePropertySlim<int> LoopCount { get; }

        ReadOnlyReactivePropertySlim<BGM> PlayingBGM { get; }

        ReadOnlyReactivePropertySlim<int> Volume { get; }
    }
}
