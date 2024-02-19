using System.Collections;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.CommonLib.Entities;
using StegoRevealer.StegoCore.CommonLib.TypeExtensions;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods;

namespace StegoRevealer.StegoCore.CommonLib
{
    /// <summary>
    /// Инструменты работы с пикселями
    /// </summary>
    public static class PixelsTools
    {
        /// <summary>
        /// Инвертировать бит по значению (возвращает новое значение)
        /// </summary>
        public static bool InvertBit(bool value)
        {
            return value ^ true;
        }

        /// <summary>
        /// Инвертировать бит по ссылке (изменяет существующее значение)
        /// </summary>
        public static void InvertBit(ref bool value)
        {
            value ^= true;
        }

        /// <summary>
        /// Инвертирование НЗБ (наименьших значащих бит)
        /// </summary>
        public static byte InvertLsb(byte value, int lsbNum = 1)
        {
            var pixel = BitArrayExtensions.NewFromByte(value);
            for (int i = 0; i < lsbNum ; i++)  // Т.к. в BitArray 0-й элемент соответствует НЗБ
            {
                pixel[i] ^= true;  // Инвертирование без обращения к специальному методу инвертирования
            }
            return pixel.AsByte();
        }

        /// <summary>
        /// Инвертирование НЗБ по ссылке (меняет переданный byte)
        /// </summary>
        public static void InvertLsb(ref byte value, int lsbNum = 1)
        {
            value = InvertLsb(value, lsbNum);
        }

        /// <summary>
        /// Инвертирование НЗБ для integer (обрезает integer до диапазона [0..255])
        /// </summary>
        public static int InvertLsb(int value, int lsbNum = 1)
        {
            return InvertLsb(value.ToByte(), lsbNum);
        }

        /// <summary>
        /// Инвертирование НЗБ для integer по ссылке (обрезает integer до диапазона [0..255])
        /// </summary>
        public static void InvertLsb(ref int value, int lsbNum = 1)
        {
            value = InvertLsb(value, lsbNum);
        }


        /// <summary>
        /// Установка определённых значений для НЗБ<br/>
        /// (если указаны 3 значения, они будут установлены последовательно в три последних бита<br/>
        /// 11010100, {0, 0, 1}  -->  11010001
        /// </summary>
        public static byte SetLsbValues(byte byteValue, params bool[] lsbValues)
        {
            if (lsbValues.Length > 8)
                throw new ArgumentException("lsbValues can't contain grater than 8 values");

            var pixel = BitArrayExtensions.NewFromByte(byteValue);
            for (int i = 0; i < lsbValues.Length; i++)
                pixel[Math.Min(lsbValues.Length, 8) - i - 1] = lsbValues[i];  // Т.к. в BitArray 0-й элемент соответствует НЗБ

            return pixel.AsByte();
        }

        /// <summary>
        /// Позволяет передавать lsbValue в виде {0,0,1}, а не {false,false,true} нативно
        /// </summary>
        public static byte SetLsbValues(byte byteValue, params int[] lsbValues)
        {
            bool[] lsbBoolValues = lsbValues.Select(v => v > 0).ToArray();
            return SetLsbValues(byteValue, lsbBoolValues);
        }

        /// <summary>
        /// Позволяет передавать lsbValue в виде {0,0,1}, а не {false,false,true} нативно<br/>
        /// byteValue будет обрезан до диапазона [0..255]
        /// </summary>
        public static byte SetLsbValues(int byteValue, params int[] lsbValues)
        {
            return SetLsbValues(byteValue.ToByte(), lsbValues);
        }

        /// <summary>
        /// Установка значений НЗБ при передаче BitArray<br/>
        /// Здесь предполагается, что lsbValues заполнен как есть, значения из lsbValues берутся последовательно с 0 индекса
        /// </summary>
        public static byte SetLsbValues(byte byteValue, BitArray lsbValues)
        {
            if (lsbValues.Length > 8)
                throw new ArgumentException("lsbValues can't contain grater than 8 values");

            var pixel = BitArrayExtensions.NewFromByte(byteValue);
            for (int i = 0; i < lsbValues.Length; i++)
                pixel[Math.Min(lsbValues.Length, 8) - i - 1] = lsbValues[i];  // Т.к. в BitArray 0-й элемент соответствует НЗБ

            return pixel.AsByte();
        }

        /// <summary>
        /// Установка значений НЗБ при передаче BitArray<br/>
        /// Здесь предполагается, что lsbValues заполнен как есть, значения из lsbValues берутся последовательно с 0 индекса
        /// </summary>
        public static byte SetLsbValues(int byteValue, BitArray lsbValues)
        {
            return SetLsbValues(byteValue.ToByte(), lsbValues);
        }

        /// <summary>
        /// Возвращает интервал с окрестностью, выходящей за пределы изображения
        /// </summary>
        /// <param name="interval"></param>
        /// <param name="requestedNeighbourhoodLength"></param>
        /// <returns></returns>
        public static byte[] GetIntervalWithNeighbourhood(ImageHandler img, ImageHorizontalIntervalInfo interval, int neighbourhoodLength = 2)
        {
            int intervalLength = interval.IntervalEndId - interval.IntervalStartId + 1;  // LeftLack: 2 - 0 + 1 = 3 | RightLack: 12 - 10 + 1 = 3
            var imgArray = img.ImgArray;
            var channelId = (int)interval.ImgChannel;

            var values = new byte[intervalLength + neighbourhoodLength * 2];  // LeftLack: [7] | RightLack: [7]
            int startInd = interval.IntervalStartId - neighbourhoodLength;  // LeftLack: 0 - 2 = -2 | RightLack: 8
            int endInd = interval.IntervalEndId + neighbourhoodLength;  // LeftLack: 2 + 2 = 4 | RightLack: 14

            // Если не хватает окрестности
            int lacksLeft = startInd < 0 ? Math.Abs(startInd) : 0;  // LeftLack: -2 < 0 => 2 | RightLack: 8 !< 0 => 0
            int lacksRight = endInd >= img.Width ? Math.Abs(endInd - img.Width + 1) : 0;  // LeftLack: 4 !>= 13 => 0 | RightLack: 14 >= 13 => |(14 - 13 + 1)| = 2

            for (int j = 0; j < lacksLeft; j++)
                values[j] = imgArray[interval.RowId, lacksLeft - j, channelId];
            for (int j = 0; j < lacksRight; j++)
                values[neighbourhoodLength + intervalLength + (neighbourhoodLength - lacksRight) + j] = imgArray[interval.RowId, img.Width - j - 2, channelId];

            // Заполняем основные
            for (int j = startInd + lacksLeft; j <= endInd - lacksRight; j++)
                values[j - startInd] = imgArray[interval.RowId, j, channelId];

            return values;
        }
    }
}
