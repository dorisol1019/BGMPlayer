using BGMPlayerCore;
using Reactive.Bindings;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BGMList.Models
{
    public class AllBGMs : IAllBGMs
    {
        public ReadOnlyReactiveProperty<List<BgmFilePath>> BGMs { get; }
        private readonly ReactiveProperty<List<BgmFilePath>> bgms;

        public AllBGMs()
        {
            bgms = new ReactiveProperty<List<BgmFilePath>>();
            BGMs = new ReadOnlyReactiveProperty<List<BgmFilePath>>(bgms);

            Refresh("./Playlist");
        }
        public void Refresh(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException();
            }

            string[]? files = Directory.GetFiles(path);
            var enableFiles = new List<BgmFilePath>();

            foreach (string? file in files.Where(e => BgmType.CanParse(e)))
            {
                var bgm = new BgmFilePath(file);
                enableFiles.Add(bgm);
            }
            bgms.Value = enableFiles;
        }
    }
}
