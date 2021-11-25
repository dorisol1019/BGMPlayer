using System.IO;

namespace BGMPlayer.Models;

public class LibrarysInfo
{
    public string Text { get; private set; }

    public LibrarysInfo()
    {
        using var reader = new StreamReader("./使用したライブラリ.txt");
        Text = reader.ReadToEnd();
    }
}
