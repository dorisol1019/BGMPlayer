using BGMPlayerCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayerOperator.Models
{
    public class ShuffledPlaylist : IPlaylist
    {
        private readonly Queue<BGM> keepPlaylist;
        private Queue<BGM> playlist;

        public ShuffledPlaylist(IEnumerable<BGM> source)
        {
            keepPlaylist = new Queue<BGM>(Shuffle(source));
            playlist = new Queue<BGM>(keepPlaylist);
        }

        private IEnumerable<BGM> Shuffle(IEnumerable<BGM> source)
        {
            return source.OrderBy(e => Guid.NewGuid());
        }
        public BGM Next()
        {
            if (!playlist.Any())
            {
                playlist = new Queue<BGM>(keepPlaylist);
            }

            return playlist.Dequeue();
        }
    }
}
