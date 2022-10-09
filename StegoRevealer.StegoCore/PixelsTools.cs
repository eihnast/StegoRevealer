using System.Collections;

namespace StegoRevealer.StegoCore
{
    public static class PixelsTools
    {
        // Инвертировать бит по значению (возвращает новое значение)
        public static bool InvertBit(bool value)
        {
            return value ^ true;
        }

        // Инвертировать бит по ссылке (изменяет существующее значение)
        public static void InvertBit(ref bool value)
        {
            value ^= true;
        }

        // Инвертирование НЗБ (наименьших значащих бит)
        public static byte InvertLsb(byte value, int lsbNum = 1)
        {
            var pixel = BitArrayExtensions.NewFromByte(value);
            for (int i = 7; i >= 7 - (Math.Min(lsbNum, 8) - 1); i--)
            {
                pixel[i] ^= true;  // Инвертирование без обращения к специальному методу инвертирования
            }
            return pixel.AsByte();
        }

        // Инвертирование НЗБ по ссылке (меняет переданный byte)
        public static void InvertLsb(ref byte value, int lsbNum = 1)
        {
            value = InvertLsb(value, lsbNum);
        }

        // Инвертирование НЗБ для integer (обрезает integer до диапазона [0..255])
        public static int InvertLsb(int value, int lsbNum = 1)
        {
            return InvertLsb(value.ToByte(), lsbNum);
        }

        public static void InvertLsb(ref int value, int lsbNum = 1)
        {
            value = InvertLsb(value, lsbNum);
        }

        

        // Установка определённых значений для НЗБ
        // (если указаны 3 значения, они будут установлены последовательно в три последних бита
        //  11010100, {0, 0, 1}  -->  11010001
        public static byte SetLsbValues(byte byteValue, params bool[] lsbValues)
        {
            if (lsbValues.Length > 8)
                throw new ArgumentException("lsbValues can't contain grater than 8 values");

            var pixel = BitArrayExtensions.NewFromByte(byteValue);
            for (int i = 0; i < lsbValues.Length; i++)
                pixel[8 - i] = lsbValues[i];

            return pixel.AsByte();
        }

        // Позволяет передавать lsbValue в виде {0,0,1}, а не {false,false,true} нативно
        public static byte SetLsbValues(byte byteValue, params int[] lsbValues)
        {
            bool[] lsbBoolValues = lsbValues.Select(v => v > 0).ToArray();
            return SetLsbValues(byteValue, lsbBoolValues);
        }

        public static byte SetLsbValues(int byteValue, params int[] lsbValues)
        {
            return SetLsbValues(byteValue.ToByte(), lsbValues);
        }

        // Установка при передаче BitArray
        public static byte SetLsbValues(byte byteValue, BitArray lsbValues)
        {
            if (lsbValues.Length > 8)
                throw new ArgumentException("lsbValues can't contain grater than 8 values");

            var pixel = BitArrayExtensions.NewFromByte(byteValue);
            for (int i = 0; i < lsbValues.Length; i++)
                pixel[7 - i] = lsbValues[i];

            return pixel.AsByte();
        }

        public static byte SetLsbValues(int byteValue, BitArray lsbValues)
        {
            return SetLsbValues(byteValue.ToByte(), lsbValues);
        }

    }
}
