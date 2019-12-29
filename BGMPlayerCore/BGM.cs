using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGMPlayerCore
{
    public class BGM
    {
        public string FileName { get; }

        public string FullPath { get; }
        public FileExtensionType FileExtension { get; }

        public BGM(string filepath, FileExtensionType fileExtension)
        {
            FullPath = Path.GetFullPath(filepath);
            FileName = Path.GetFileName(filepath);
            FileExtension = fileExtension;
        }
    }
}
