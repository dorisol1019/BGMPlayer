using BGMPlayerCore;
using System.Collections.Generic;

namespace PlayerOperator.Models
{
    public class OrderedPlaylist : IPlaylist
    {
        private readonly List<BGM> playlist;
        private int currentIndex = 0;
        public OrderedPlaylist(IEnumerable<BGM> source, BGM currentBGM)
        {
            playlist = new List<BGM>(source);
            currentIndex = playlist.FindIndex(e => e == currentBGM);
        }
        public BGM Next()
        {
            currentIndex++;
            int index = currentIndex % playlist.Count;

            if (currentIndex >= playlist.Count)
            {
                currentIndex = 0;
            }

            return playlist[index];
        }
    }
}
