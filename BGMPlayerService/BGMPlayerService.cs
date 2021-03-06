using BGMPlayerCore;
using Reactive.Bindings;
using System.Threading.Tasks;

namespace BGMPlayer
{
    public class BGMPlayerService : IBGMPlayerService
    {
        private readonly IBGMPlayerCoreApi bgmPlayerCore;

        public ReadOnlyReactivePropertySlim<PlayingState> State { get; }

        public ReadOnlyReactivePropertySlim<int> LoopCounter { get; }

        public ReadOnlyReactivePropertySlim<bool> IsPlaying { get; }

        public ReadOnlyReactivePropertySlim<BGM> PlayingBGM { get; }

        public ReadOnlyReactivePropertySlim<int> Volume { get; }

        private readonly ReactivePropertySlim<bool> isPlaying;

        public BGMPlayerService(IBGMPlayerCoreApi bgmPlayerCore)
        {
            this.bgmPlayerCore = bgmPlayerCore;

            State = bgmPlayerCore.State.ToReadOnlyReactivePropertySlim();

            LoopCounter = bgmPlayerCore.LoopCount.ToReadOnlyReactivePropertySlim();

            isPlaying = new ReactivePropertySlim<bool>(false);
            IsPlaying = isPlaying.ToReadOnlyReactivePropertySlim();

            PlayingBGM = this.bgmPlayerCore.PlayingBGM;

            Volume = this.bgmPlayerCore.Volume;
        }
        public async Task Play(BGM bgm)
        {
            bgmPlayerCore.Stop();
            await bgmPlayerCore.Play(bgm);
            isPlaying.Value = true;
        }

        public void Stop()
        {
            bgmPlayerCore.Stop();
            isPlaying.Value = false;
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

        public void Dispose()
        {
            bgmPlayerCore.Dispose();
        }
    }
}
