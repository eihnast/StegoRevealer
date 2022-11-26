using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegoRevealer.WinUi.Lib
{
    [Flags]
    public enum AccessMode
    {
        None = 0,
        Get = 1,
        Set = 2
    }
}
