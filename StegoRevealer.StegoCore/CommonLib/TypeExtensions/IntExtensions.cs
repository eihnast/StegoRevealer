namespace StegoRevealer.StegoCore.CommonLib.TypeExtensions;

/// <summary>
/// Расширения класса Int
/// </summary>
public static class IntExtensions
{
    /// <summary>
    /// Обрезает int до диапазона [0.255]
    /// </summary>
    public static byte ToByte(this int value)
    {
        byte byteValue = (byte)value;
        if (value > 255)
            byteValue = 255;
        else if (value < 0)
            byteValue = 0;

        return byteValue;
    }
}
