using BGMPlayerCore;
using System.Collections.Generic;

namespace PlayerOperator.Models
{
    public class OrderedPlaylist : IPlaylist
    {
        private readonly List<BgmFilePath> playlist;
        private int currentIndex = 0;
        public OrderedPlaylist(IEnumerable<BgmFilePath> source, BgmFilePath currentBGM)
        {
            playlist = new List<BgmFilePath>(source);
            currentIndex = playlist.FindIndex(e => e == currentBGM);
        }
        public BgmFilePath Next()
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
