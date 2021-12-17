using BGMPlayerCore;

namespace PlayerOperator.Models;

public class ShuffledPlaylist : IPlaylist
{
    private List<BgmFilePath> playlist;

    private int currentIndex = 0;

    public ShuffledPlaylist(IEnumerable<BgmFilePath> source)
    {
        playlist = new List<BgmFilePath>(Shuffle(source));
        currentIndex = 0;
    }

    public ShuffledPlaylist(IEnumerable<BgmFilePath> source, BgmFilePath currentBGM)
    {
        playlist = new List<BgmFilePath>(Shuffle(source));
        currentIndex = playlist.FindIndex(e => e == currentBGM);
    }

    private IEnumerable<BgmFilePath> Shuffle(IEnumerable<BgmFilePath> source)
    {
        return source.OrderBy(e => Guid.NewGuid());
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

        if (currentIndex < 0)
        {
            currentIndex = playlist.Count - 1;
        }

        int index = currentIndex % playlist.Count;

        return playlist[index];
    }
}
