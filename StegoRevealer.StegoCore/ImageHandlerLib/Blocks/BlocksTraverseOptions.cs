using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.StegoMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoRevealer.StegoCore.ImageHandlerLib.Blocks
{
    public class BlocksTraverseOptions
    {
        public UniqueList<ImgChannel> Channels { get; set; }
        public StartValues StartBlocks { get; set; }
        public TraverseType TraverseType { get; set; } = TraverseType.Horizontal;
        public bool InterlaceChannels { get; set; } = false;
        public int? Seed { get; set; } = null;
    }
}
