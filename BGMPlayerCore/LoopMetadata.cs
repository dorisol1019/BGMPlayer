using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGMPlayerCore
{
    public record LoopMetadata
    {
        public long Start { get; init; }
        public long Length{ get; init; }
    }
}
