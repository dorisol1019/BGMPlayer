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
        public BGMPlayerService(IBGMPlayerCore bgmPlayerCore)
        {
            this.bgmPlayerCore = bgmPlayerCore;
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
            throw new NotImplementedException();
        }
    }
}
