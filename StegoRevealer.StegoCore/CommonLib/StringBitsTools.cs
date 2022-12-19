using System.Text;
using System.Collections;
using StegoRevealer.StegoCore.CommonLib.TypeExtensions;

namespace StegoRevealer.StegoCore.CommonLib
{
    /// <summary>
    /// Инструменты для работы с преобразованиями между String и BitArray
    /// </summary>
    public static class StringBitsTools
    {
        /// <summary>
        /// Преобразование string в массив бит (BitArray), кодировка UTF-8
        /// </summary>
        public static BitArray StringToBitArray(string data)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            return new BitArray(byteArray);
        }

        /// <summary>
        /// Преобразование массива бит (BitArray) в строку, кодировка UTF-8
        /// </summary>
        public static string BitArrayToString(BitArray bitArray)
        {
            byte[] byteArray = bitArray.ToBytes();
            return Encoding.UTF8.GetString(byteArray);
        }
    }
}
