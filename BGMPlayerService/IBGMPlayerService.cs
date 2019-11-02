﻿using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BGMPlayerCore;

namespace BGMPlayer
{
    public interface IBGMPlayerService : IDisposable
    {
        Task Play(BGM bgm);

        void Stop();

        void PauseOrReStart();

        void ChangeVolume(int value);

        ReadOnlyReactivePropertySlim<PlayingState> State { get; }

        ReadOnlyReactivePropertySlim<int> LoopCounter { get; }

        ReadOnlyReactivePropertySlim<bool> IsPlaying { get; }

        ReadOnlyReactivePropertySlim<BGM> PlayingBGM { get; }

        ReadOnlyReactivePropertySlim<int> Volume { get; }
    }
}
