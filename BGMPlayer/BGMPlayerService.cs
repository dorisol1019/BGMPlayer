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

        private BooleanNotifier IsPause = default;

        public ReadOnlyReactivePropertySlim<PlayingState> State { get; }
        public BGMPlayerService(IBGMPlayerCore bgmPlayerCore)
        {
            this.bgmPlayerCore = bgmPlayerCore;

            IsPause = new BooleanNotifier(false);

            State = bgmPlayerCore.State.ToReadOnlyReactivePropertySlim();
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
            if(!IsPause.Value)
            {
                bgmPlayerCore.Pause();
                IsPause.TurnOn();
            }
            else if (IsPause.Value)
            {
                bgmPlayerCore.ReStart();
                IsPause.TurnOff();
            }
        }
    }
}
