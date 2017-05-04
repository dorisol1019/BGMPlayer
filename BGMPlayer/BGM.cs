using BGMPlayer.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGMPlayer
{
    public class BGM
    {
        public string FileName { get; }
        public FileExtensionType FileExtension { get; }

        public BGM(string fileName,FileExtensionType fileExtension)
        {
            FileName = fileName;
            FileExtension = fileExtension;
        }
    }
}
