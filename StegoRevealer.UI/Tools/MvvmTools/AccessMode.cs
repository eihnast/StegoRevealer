using System;

namespace StegoRevealer.UI.Tools.MvvmTools
{
    /// <summary>
    /// Разрешённый способ доступа
    /// </summary>
    [Flags]
    public enum AccessMode
    {
        None = 0,
        Get = 1,
        Set = 2
    }
}
