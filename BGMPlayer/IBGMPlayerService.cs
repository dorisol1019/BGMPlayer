using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGMPlayer
{
    public interface IBGMPlayerService
    {
        Task Play(BGM bgm);

        void Stop();

        void PauseOrReStart();

        void ChangeVolume(int value);

        ReadOnlyReactivePropertySlim<PlayingState> State { get; }

        ReadOnlyReactivePropertySlim<int> LoopCounter { get; }

    }
}
