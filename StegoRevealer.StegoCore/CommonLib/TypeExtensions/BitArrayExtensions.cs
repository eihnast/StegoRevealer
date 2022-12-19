using System.Collections;

namespace StegoRevealer.StegoCore.CommonLib.TypeExtensions
{
    /// <summary>
    /// Расширения класса BitArray
    /// </summary>
    public static class BitArrayExtensions
    {
        /// <summary>
        /// Содержит ли BitArray только один байт
        /// </summary>
        public static bool IsOneByte(this BitArray bitArray)
        {
            return bitArray.Length == 8;
        }

        /// <summary>
        /// Преобразование в байт (если хранит больше одного байта - будет возвращён только первый байт)
        /// </summary>
        public static byte AsByte(this BitArray bitArray)
        {
            byte[] resultArray = new byte[1];
            bitArray.CopyTo(resultArray, 0);
            return resultArray[0];
        }

        /// <summary>
        /// Возвращает байт в виде integer
        /// </summary>
        public static int AsInt(this BitArray value)
        {
            return value.AsByte();
        }

        /// <summary>
        /// Возвращает массив байт
        /// </summary>
        public static byte[] ToBytes(this BitArray bitArray)
        {
            int bytesNum = bitArray.Length / 8;
            var extraByte = bitArray.Length % 8 != 0;

            byte[] resultArray = new byte[bytesNum + (extraByte ? 1 : 0)];
            bitArray.CopyTo(resultArray, 0);

            if (extraByte)
                return resultArray[0..^1];
            else
                return resultArray;
        }

        /// <summary>
        /// Новый BitArray из одного байта
        /// </summary>
        public static BitArray NewFromByte(byte value)
        {
            var bitArray = new BitArray(new byte[] { value });
            return bitArray;
        }

        /// <summary>
        /// Новый BitArray из одного integer (обрезает значения до диапазона [0..255])
        /// </summary>
        public static BitArray NewFromInt(int value)
        {
            return NewFromByte(value.ToByte());
        }
    }
}
