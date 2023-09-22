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
        /// Преобразование string в массив бит (BitArray), кодировка UTF-8<br/>
        /// Сохраняет прямую последовательность бит в BitArray: от 0-го к N-му байту, от 0-го бита к 8-му по каждому байту
        /// </summary>
        public static BitArray StringToBitArray(string data, bool linearBitArrays = true)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            var bitArray = new BitArray(byteArray);

            if (linearBitArrays)
            {
                // Т.к. в BitArray 0-й элемент соответствует НЗБ
                var correctBitArray = new BitArray(byteArray.Length * 8);
                for (int i = 0; i < byteArray.Length; i++)
                    for (int j = 0; j < 8; j++)
                        correctBitArray[i * 8 + j] = bitArray[i * 8 + (7 - j)];
                return correctBitArray;
            }

            return bitArray;
        }

        /// <summary>
        /// Преобразование массива бит (BitArray) в строку, кодировка UTF-8<br/>
        /// Принимает на вход прямую последовательность бит в BitArray: от 0-го к N-му байту, от 0-го бита к 8-му по каждому байту.
        /// Обрезает при конвертации "хвост" из менее, чем 8-ми последних битов
        /// </summary>
        public static string BitArrayToString(BitArray bitArray, bool linearBitArrays = true)
        {
            BitArray actualBitArray = bitArray;
            if (linearBitArrays)
            {
                // Т.к. в BitArray по умолчанию 0-й элемент соответствует НЗБ
                var reversedBitArray = new BitArray(bitArray.Length);
                for (int i = 0; i < bitArray.Length / 8; i++)
                    for (int j = 0; j < 8; j++)
                        reversedBitArray[i * 8 + j] = bitArray[i * 8 + (7 - j)];
                actualBitArray = reversedBitArray;
            }

            byte[] byteArray = actualBitArray.ToBytes();  // Предполагаем, что биты уже в типичном порядке: НЗБ в 0-м индексе в рамках байта
            return Encoding.UTF8.GetString(byteArray);
        }
    }
}
