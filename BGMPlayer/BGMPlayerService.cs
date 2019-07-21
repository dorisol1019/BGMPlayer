using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGMPlayer
{
    public enum PlayingState
    {
        Playing,
        Stopping,
        Pausing,
    }
    public class BGMPlayerService : IBGMPlayerService
    {
        private IBGMPlayerCore bgmPlayerCore = default;

        private BooleanNotifier IsPause = default;
        public BGMPlayerService(IBGMPlayerCore bgmPlayerCore)
        {
            this.bgmPlayerCore = bgmPlayerCore;

            IsPause = new BooleanNotifier(false);
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
