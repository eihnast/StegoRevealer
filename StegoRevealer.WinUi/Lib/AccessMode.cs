using System;

namespace StegoRevealer.WinUi.Lib
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
