﻿using System.Collections;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.CommonLib.TypeExtensions;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StegoRevealer.StegoCore.StegoMethods.Lsb;

/// <summary>
/// Метод извлечения информации из НЗБ
/// </summary>
public class LsbExtractor : IExtractor
{
    /// <summary>
    /// Параметры метода НЗБ
    /// </summary>
    public LsbParameters Params { get; set; }


    public LsbExtractor(ImageHandler img, 
        int? seed = null, bool interlaceChannels = true, TraverseType traverseType = TraverseType.Horizontal, int lsbNum = 1)
    {
        Params = new LsbParameters(img);
        Params.StegoOperation = StegoOperationType.Extracting;
        Params.Seed = seed;
        Params.InterlaceChannels = interlaceChannels;
        Params.TraverseType = traverseType;
        Params.LsbNum = lsbNum;
    }


    /// <inheritdoc/>
    public IExtractResult Extract() => ExtractAlgorithm();

    /// <inheritdoc/>
    public IExtractResult Extract(IParams parameters)
    {
        LsbExtractResult result = new();

        LsbParameters? lsbParams = parameters as LsbParameters;
        if (lsbParams is null)  // Не удалось привести к LsbParameters
        {
            result.Error("kzParams является null");
            return result;
        }

        // Замена параметров на переданные
        var oldLsbParams = Params;
        Params = lsbParams;

        result = ExtractAlgorithm();
        Params = oldLsbParams;  // Возврат параметров
        return result;
    }

    // Логика метода с текущими параметрами
    private LsbExtractResult ExtractAlgorithm()
    { 
        LsbExtractResult result = new();
        result.Log($"Запущен процесс извлечения из {Params.Image.ImgName}");

        List<bool> dataBitArray = new();  // Массив извлечённых данных

        // Доопределение параметров извлечения
        bool isRandomHiding = Params.Seed is not null;  // Вид скрытия: последовательный или псевдослучайный
        int usedColorBytesNum = Params.ToExtractColorBytesNum;  // Количество извлекаемых байт цвета
        result.Log($"Установлены параметры:\n\t" +
            $"isRandomHiding = {isRandomHiding}\n\tusedColorBytesNum = {usedColorBytesNum}");

        // Выбор типа итерации в зависимости от метода скрытия (последовательное / псевдослучайное)
        Func<int, LsbParameters, IEnumerable<ScPointCoords>> iterator
            = isRandomHiding ? LsbCommon.GetForRandomAccessIndex : LsbCommon.GetForLinearAccessIndex;

        // Осуществление извлечения
        result.Log("Запущен цикл извлечения");
        foreach (var colorByteCoords in iterator(usedColorBytesNum, Params))
        {
            (int line, int column, int channel) = colorByteCoords.AsTuple();
            byte colorByte = Params.Image.ImgArray[line, column, channel];
            int remaining = Params.ToExtractBitLength - dataBitArray.Count;
            BitArray extractedBits = ExtractBitsFromColorByte(colorByte, Params.LsbNum, remaining);
            for (int i = 0; i < extractedBits.Length; i++)
                dataBitArray.Add(extractedBits[i]);
        }
        result.Log("Завершён цикл извлечения");

        // Преобразование извлечённых бит в текст
        result.ResultData = StringBitsTools.BitArrayToString(new BitArray(dataBitArray.ToArray()), linearBitArrays: true);
        result.Log($"Объём извлечённой информации: {result.ResultData.Length} символов");

        result.Log("Процесс извлечения завершён");
        return result;
    }

    /// <summary>
    /// Метод извлечения бита из цветового байта
    /// </summary>
    private static BitArray ExtractBitsFromColorByte(byte colorByte, int lsbNum, int remainingBits)
    {
        var bits = new BitArray(Math.Min(lsbNum, remainingBits));
        var colorByteAsBits = BitArrayExtensions.NewFromByte(colorByte, linearOrder: true);
        for (int i = 0; i < lsbNum && i < remainingBits; i++)  // Проверяем также, что не превысили число оставшихся бит
            bits[i] = colorByteAsBits[8 - lsbNum + i];
        return bits;
    }
}
