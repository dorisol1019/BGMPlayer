using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BGMPlayerCore;

namespace PlayerOperator.Models
{
    public class OrderedPlaylist : IPlaylist
    {
        private List<BGM> playlist;
        private int currentIndex = 0;
        public OrderedPlaylist(IEnumerable<BGM> source)
        {
            playlist = new List<BGM>(source);
        }
        public BGM Next()
        {
            int index = currentIndex % playlist.Count;
            currentIndex++;

            if(currentIndex >= playlist.Count)
            {
                currentIndex = 0;
            }

            return playlist[index];
        }
    }
}
