using System.IO;

namespace BGMPlayerCore
{
    public class BgmFilePath
    {
        public string FileName { get; }

        public string FullPath { get; }
        public FileExtensionType FileExtension { get; }

        public BgmFilePath(string filepath, FileExtensionType fileExtension)
        {
            FullPath = Path.GetFullPath(filepath);
            FileName = Path.GetFileName(filepath);
            FileExtension = fileExtension;
        }
    }
}
