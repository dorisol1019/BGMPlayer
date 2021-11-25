using BGMPlayerCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayerOperator.Models
{
    public class ShuffledPlaylist : IPlaylist
    {
        private readonly Queue<BgmFilePath> keepPlaylist;
        private Queue<BgmFilePath> playlist;

        public ShuffledPlaylist(IEnumerable<BgmFilePath> source)
        {
            keepPlaylist = new Queue<BgmFilePath>(Shuffle(source));
            playlist = new Queue<BgmFilePath>(keepPlaylist);
        }

        private IEnumerable<BgmFilePath> Shuffle(IEnumerable<BgmFilePath> source)
        {
            return source.OrderBy(e => Guid.NewGuid());
        }
        public BgmFilePath Next()
        {
            if (!playlist.Any())
            {
                playlist = new Queue<BgmFilePath>(keepPlaylist);
            }

            return playlist.Dequeue();
        }
    }
}
