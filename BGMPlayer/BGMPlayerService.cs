using Reactive.Bindings;
using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGMPlayer
{
    public class BGMPlayerService : IBGMPlayerService
    {
        private IBGMPlayerCore bgmPlayerCore = default;

        public ReadOnlyReactivePropertySlim<PlayingState> State { get; }

        public ReadOnlyReactivePropertySlim<int> LoopCounter{get;}
        public BGMPlayerService(IBGMPlayerCore bgmPlayerCore)
        {
            this.bgmPlayerCore = bgmPlayerCore;

            State = bgmPlayerCore.State.ToReadOnlyReactivePropertySlim();

            // LoopCountの作りが特殊なのでこのままではいけない
            LoopCounter = bgmPlayerCore.LoopCount.ToReadOnlyReactivePropertySlim();
        }
        public async Task Play(BGM bgm)
        {
            bgmPlayerCore.Stop();
            await bgmPlayerCore.Play(bgm);
        }

        public void Stop()
        {
            bgmPlayerCore.Stop();
        }

        public void PauseOrReStart()
        {
            switch (State.Value)
            {
                case PlayingState.Playing:
                    bgmPlayerCore.Pause();
                    break;
                case PlayingState.Pausing:
                    bgmPlayerCore.ReStart();
                    break;
                default:
                    break;
            }
        }

        public void ChangeVolume(int value)
        {
            bgmPlayerCore.ChangeVolume(value);
        }
    }
}
