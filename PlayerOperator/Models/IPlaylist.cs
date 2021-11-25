using BGMPlayerCore;

namespace PlayerOperator.Models
{
    public interface IPlaylist
    {
        BgmFilePath Next();
    }
}
