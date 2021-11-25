using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BGMPlayerCore
{
    /// <summary>
    /// Bgmの種類を表すValueObject。
    /// </summary>

    public record BgmType
    {
        public static readonly BgmType Midi = new(".midi", new[] { ".midi", ".mid" });
        public static readonly BgmType Wave = new(".wave", new[] { ".wave", ".wav" });
        public static readonly BgmType MP3 = new(".mp3");
        public static readonly BgmType Ogg = new(".ogg");

        BgmType(string id) : this(id, new[] { id })
        {
        }

        BgmType(string id, string[] extensions)
        {
            Id = id.ToLower();
            Extensions = extensions.Select(e => e.ToLower()).ToArray();
        }

        public string[] Extensions { get; }
        public string Id { get; }

        private bool CanParseFileExtension(string fileExtension)
        {
            return Extensions.Contains(fileExtension.ToLower());
        }

        public static IEnumerable<BgmType> GetValues()
        {
            yield return Midi;
            yield return Wave;
            yield return MP3;
            yield return Ogg;
        }

        public static BgmType Parse(string filePath)
        {
            string fileExtentionName = Path.GetExtension(filePath);
            foreach (var value in GetValues())
            {
                if (value.CanParseFileExtension(fileExtentionName))
                {
                    return value;
                }
            }

            throw new NotSupportedException($"{filePath} は有効なBgmTypeではありません。");
        }

        public static bool CanParse(string filePath)
        {
            string fileExtentionName = Path.GetExtension(filePath);
            foreach (var value in GetValues())
            {
                if (value.CanParseFileExtension(fileExtentionName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
