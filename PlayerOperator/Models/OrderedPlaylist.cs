using BGMPlayerCore;

namespace PlayerOperator.Models;

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

    public BgmFilePath Previous()
    {
        currentIndex--;

        if(currentIndex < 0)
        {
            currentIndex = playlist.Count - 1;
        }

        int index = currentIndex % playlist.Count;

        return playlist[index];
    }
}
