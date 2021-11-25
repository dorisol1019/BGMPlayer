using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BGMPlayerCore
{
    public class BgmFilePath
    {
        public string FileName { get; }

        public string FullPath { get; }
        public BgmType BgmType { get; }

        public BgmFilePath(string filepath)
        {
            FullPath = Path.GetFullPath(filepath);
            FileName = Path.GetFileName(filepath);
            BgmType = BgmType.Parse(filepath);
        }
    }
}
