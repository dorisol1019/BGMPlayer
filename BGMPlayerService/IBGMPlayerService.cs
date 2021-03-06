﻿using BGMPlayerCore;
using Reactive.Bindings;
using System;
using System.Threading.Tasks;

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
