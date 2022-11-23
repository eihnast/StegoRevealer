namespace StegoRevealer.StegoCore.CommonLib.TypeExtensions
{
    public static class IntExtensions
    {
        // Обрезает int до диапазона [0.255]
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
}
