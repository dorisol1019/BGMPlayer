﻿using BGMPlayerCore;
using Reactive.Bindings;
using System.Collections.Generic;
using System.IO;

namespace BGMList.Models
{
    public class AllBGMs : IAllBGMs
    {
        public ReadOnlyReactiveProperty<List<BGM>> BGMs { get; }
        private readonly ReactiveProperty<List<BGM>> bgms;

        public AllBGMs()
        {
            bgms = new ReactiveProperty<List<BGM>>();
            BGMs = new ReadOnlyReactiveProperty<List<BGM>>(bgms);

            Refresh("./Playlist");
        }
        public void Refresh(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException();
            }

            string[]? files = Directory.GetFiles(path);
            var enableFiles = new List<BGM>();

            foreach (string? file in files)
            {
                foreach (string? extensionName in new[] { ".mid", ".midi", ".wav", ".wave", ".mp3", ".ogg" })
                {
                    string? extension = Path.GetExtension(file);
                    if (string.Compare(extension, extensionName, true) == 0)
                    {
                        FileExtensionType ext = FileExtensionType.other;
                        extension = extension.ToLower();
                        if (extension == ".mid" || extension == ".midi")
                        {
                            ext = FileExtensionType.midi;
                        }

                        if (extension == ".wav" || extension == ".wave")
                        {
                            ext = FileExtensionType.wave;
                        }

                        if (extension == ".mp3")
                        {
                            ext = FileExtensionType.mp3;
                        }

                        if (extension == ".ogg")
                        {
                            ext = FileExtensionType.ogg;
                        }

                        var bgm = new BGM(file, ext);
                        enableFiles.Add(bgm);
                        break;
                    }
                }
            }
            bgms.Value = enableFiles;
        }
    }
}
