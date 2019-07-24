using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGMPlayer
{
    public interface IBGMPlayerCore : IDisposable
    {
        Task Play(BGM bgm);

        void Stop();

        void Pause();

        void ReStart();

        void ChangeVolume(int volume);

        ReactivePropertySlim<PlayingState> State { get; }

        ReadOnlyReactiveProperty<int> LoopCount { get; }

    }
}
