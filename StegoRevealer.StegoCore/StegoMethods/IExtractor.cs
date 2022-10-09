using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoRevealer.StegoCore.StegoMethods
{
    public interface IExtractor
    {
        public IExtractResult Extract(IParams parameters);
        public IExtractResult Extract();
    }
}
