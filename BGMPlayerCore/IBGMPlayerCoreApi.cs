using Reactive.Bindings;
using System;
using System.Threading.Tasks;

namespace BGMPlayerCore
{
    public interface IBGMPlayerCoreApi : IDisposable
    {
        Task Play(BgmFilePath bgm);

        void Stop();

        void Pause();

        void ReStart();

        void ChangeVolume(int volume);

        ReadOnlyReactivePropertySlim<PlayingState> State { get; }

        ReadOnlyReactivePropertySlim<int> LoopCount { get; }

        ReadOnlyReactivePropertySlim<BgmFilePath> PlayingBGM { get; }

        ReadOnlyReactivePropertySlim<int> Volume { get; }
    }
}
