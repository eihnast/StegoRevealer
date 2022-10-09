using System.Collections;

namespace StegoRevealer.StegoCore
{
    public static class BitArrayExtensions
    {
        // Содержит ли BitArray только один байт
        public static bool IsOneByte(this BitArray bitArray)
        {
            return bitArray.Length == 8;
        }

        // Преобразование в байт (если хранит больше одного байта - будет возвращён только первый байт)
        public static byte AsByte(this BitArray bitArray)
        {
            byte[] resultArray = new byte[1];
            bitArray.CopyTo(resultArray, 0);
            return resultArray[0];
        }

        // Возвращает байт в виде integer
        public static int AsInt(this BitArray value)
        {
            return (int)value.AsByte();
        }

        // Возвращает массив байт
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

        // Новый BitArray из одного байта
        public static BitArray NewFromByte(byte value)
        {
            var bitArray = new BitArray(new byte[] { value });
            return bitArray;
        }

        // Новый BitArray из одного integer (обрезает значения до диапазона [0..255])
        public static BitArray NewFromInt(int value)
        {
            return NewFromByte(value.ToByte());
        }
    }
}
