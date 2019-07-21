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
        public BGMPlayerService(IBGMPlayerCore bgmPlayerCore)
        {
            this.bgmPlayerCore = bgmPlayerCore;

            IsPause = new BooleanNotifier(false);
        }
        public Task Play(BGM bgm)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
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
