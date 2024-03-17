using System.Collections;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.CommonLib.Entities;
using StegoRevealer.StegoCore.CommonLib.TypeExtensions;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ScMath;
using StegoRevealer.StegoCore.StegoMethods;

namespace StegoRevealer.StegoCore.CommonLib;

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

    /// <summary>
    /// Возвращает интервал с окрестностью, выходящей за пределы изображения
    /// </summary>
    /// <param name="interval"></param>
    /// <param name="requestedNeighbourhoodLength"></param>
    /// <returns></returns>
    public static byte[] GetIntervalWithNeighbourhood(byte[,] pixelsArray, ImageHorizontalIntervalInfo interval, int neighbourhoodLength = 2)
    {
        int intervalLength = interval.IntervalEndId - interval.IntervalStartId + 1;  // LeftLack: 2 - 0 + 1 = 3 | RightLack: 12 - 10 + 1 = 3

        int width = pixelsArray.GetLength(1);

        var values = new byte[intervalLength + neighbourhoodLength * 2];  // LeftLack: [7] | RightLack: [7]
        int startInd = interval.IntervalStartId - neighbourhoodLength;  // LeftLack: 0 - 2 = -2 | RightLack: 8
        int endInd = interval.IntervalEndId + neighbourhoodLength;  // LeftLack: 2 + 2 = 4 | RightLack: 14

        // Если не хватает окрестности
        int lacksLeft = startInd < 0 ? Math.Abs(startInd) : 0;  // LeftLack: -2 < 0 => 2 | RightLack: 8 !< 0 => 0
        int lacksRight = endInd >= width ? Math.Abs(endInd - width + 1) : 0;  // LeftLack: 4 !>= 13 => 0 | RightLack: 14 >= 13 => |(14 - 13 + 1)| = 2

        for (int j = 0; j < lacksLeft; j++)
            values[j] = pixelsArray[interval.RowId, lacksLeft - j];
        for (int j = 0; j < lacksRight; j++)
            values[neighbourhoodLength + intervalLength + (neighbourhoodLength - lacksRight) + j] = pixelsArray[interval.RowId, width - j - 2];

        // Заполняем основные
        for (int j = startInd + lacksLeft; j <= endInd - lacksRight; j++)
            values[j - startInd] = pixelsArray[interval.RowId, j];

        return values;
    }

    /// <summary>Ограничивает значение [0..255] и возвращает как байт, отсекая дробную часть</summary>
    public static byte ToLimitedByte(double value) => (byte)Math.Max(0, Math.Min(value, 255));

    /// <summary>Ограничивает значение [0..255] и возвращает как байт, отсекая дробную часть</summary>
    public static byte ToLimitedByte(float value) => (byte)Math.Max(0, Math.Min(value, 255));

    public static double[,] ByteToDoubleMatrix(byte[,] matrix)
    {
        var doubleMatrix = new double[matrix.GetLength(0), matrix.GetLength(1)];
        for (int i = 0; i < matrix.GetLength(0); i++)
            for (int j = 0; j < matrix.GetLength(1); j++)
                doubleMatrix[i, j] = (double)matrix[i, j];
        return doubleMatrix;
    }

    public static byte[,] DoubleToByteMatrix(double[,] matrix)
    {
        var byteMatrix = new byte[matrix.GetLength(0), matrix.GetLength(1)];
        for (int i = 0; i < matrix.GetLength(0); i++)
            for (int j = 0; j < matrix.GetLength(1); j++)
                byteMatrix[i, j] = ToLimitedByte(matrix[i, j]);
        return byteMatrix;
    }

    // Переводит цветовой байт (RGB) в grayscale
    private static byte ToGrayscaleByte(byte[] rgb, bool asAverage = true)
    {
        if (rgb.Length != 3)
            throw new Exception("RGB array must contains only 3 byte values!");

        byte result = 0;

        if (asAverage)
            result = MathMethods.Average(rgb);
        else
            result = PixelsTools.ToLimitedByte(rgb[0] * .21f + rgb[1] * .71f + rgb[2] * .071f);
        return result;
    }

    public static byte[,] ToGrayscale(ImageArray imageArray, bool useAveragedGrayscale = true)
    {
        int height = imageArray.Height;
        int width = imageArray.Width;

        // Перевод в grayscale
        var gimar = new byte[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var fullByte = imageArray[y, x];
                var rgb = new byte[] { fullByte.Red, fullByte.Green, fullByte.Blue };
                var gbyte = ToGrayscaleByte(rgb, useAveragedGrayscale);
                gimar[y, x] = gbyte;
            }
        }

        return gimar;
    }

    public static byte[,] ToGrayscale(byte[,,] pixelsArray, bool useAveragedGrayscale = true)
    {
        int height = pixelsArray.GetLength(0);
        int width = pixelsArray.GetLength(1);
        int deep = pixelsArray.GetLength(2);
        if (deep != 3)
            throw new Exception("Can't calculate grayscale byte if there not 3 bytes of pixel in origin array");

        // Перевод в grayscale
        var gimar = new byte[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var fullByte = new ScPixel(pixelsArray[y, x, 0], pixelsArray[y, x, 1], pixelsArray[y, x, 2]);
                var rgb = new byte[] { fullByte.Red, fullByte.Green, fullByte.Blue };
                var gbyte = ToGrayscaleByte(rgb, useAveragedGrayscale);
                gimar[y, x] = gbyte;
            }
        }

        return gimar;
    }

    // Bresenham's Line Algorithm
    public static List<PixelInfo> GetPixelsOnLine(int y0, int x0, int y1, int x1)
    {
        var pixels = new List<PixelInfo>();

        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            int t;
            t = x0;  // swap x0 and y0
            x0 = y0;
            y0 = t;
            t = x1;  // swap x1 and y1
            x1 = y1;
            y1 = t;
        }

        if (x0 > x1)
        {
            int t;
            t = x0;  // swap x0 and x1
            x0 = x1;
            x1 = t;
            t = y0;  // swap y0 and y1
            y0 = y1;
            y1 = t;
        }

        int dx = x1 - x0;
        int dy = Math.Abs(y1 - y0);
        int error = dx / 2;
        int ystep = (y0 < y1) ? 1 : -1;

        int y = y0;
        for (int x = x0; x <= x1; x++)
        {
            pixels.Add(new PixelInfo { Y = steep ? x : y, X = steep ? y : x });

            error = error - dy;
            if (error < 0)
            {
                y += ystep;
                error += dx;
            }
        }

        return pixels;
    }
}
