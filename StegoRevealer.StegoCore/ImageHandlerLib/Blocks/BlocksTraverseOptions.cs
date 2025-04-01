using Accord;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.StegoMethods;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;

namespace StegoRevealer.StegoCore.ImageHandlerLib.Blocks;

/*
 * Целевое решение: создавать параметры обхода блоков на основе параметров метода непосредственно для получения
 *   итератора по матрице блоков, когда это необходимо.
 * Создавать объект параметров обхода как внутренний параметр хранения опций в параметрах методов не рекомендуется.
 */

/// <summary>
/// DTO-класс параметров поблочного обхода изображения
/// </summary>
public class BlocksTraverseOptions
{
    private UniqueList<ImgChannel> _channels = new();
    private StartValues _startValues = StartValues.GetZeroStartValues();

    /// <summary>
    /// Список каналов
    /// </summary>
    public UniqueList<ImgChannel> Channels 
    {
        get => _channels;
        set
        {
            if (value is not null)
                _channels = CloneChannelsList(value);
        }
    }

    /// <summary>
    /// Список начальных значений
    /// </summary>
    public StartValues StartBlocks 
    {
        get => _startValues;
        set
        {
            if (value is not null)
                _startValues = CloneStartValues(value);
        }
    }

    /// <summary>
    /// Тип обхода
    /// </summary>
    public TraverseType TraverseType { get; set; } = TraverseType.Horizontal;

    /// <summary>
    /// Флаг включения чересканального обхода
    /// </summary>
    public bool InterlaceChannels { get; set; } = false;

    /// <summary>
    /// Ключ генератора ГПСЧ<br/>
    /// Если не null, метод обхода будет считаться псевдослучайным
    /// </summary>
    public int? Seed { get; set; }


    // Конструкторы

    /// <summary>
    /// Создаёт параметры обхода с указанными значениями<br/>
    /// Передача null (или отсутствие передачи) любого параметра, кроме <paramref name="seed"/> будет означать
    /// установку значения для этого параметра по умолчанию
    /// </summary>
    public BlocksTraverseOptions(
        UniqueList<ImgChannel>? channels = null, StartValues? startBlocks = null, TraverseType? traverseType = null, 
        bool? interlaceChannels = null, int? seed = null)
    {
        if (channels is not null)
            Channels = channels;
        if (startBlocks is not null)
            StartBlocks = startBlocks;
        if (traverseType.HasValue)
            TraverseType = traverseType.Value;
        if (interlaceChannels.HasValue)
            InterlaceChannels = interlaceChannels.Value;
        Seed = seed;
    }

    /// <summary>
    /// Создаёт параметры обхода из параметров метода Коха-Жао
    /// </summary>
    public BlocksTraverseOptions(KochZhaoParameters parameters)
    {
        Channels = CloneChannelsList(parameters.Channels);
        StartBlocks = CloneStartValues(parameters.StartBlocks);
        TraverseType = parameters.TraverseType;
        InterlaceChannels = parameters.InterlaceChannels;
        Seed = parameters.Seed;
    }


    // Вспомогательные методы

    // Клонирует список каналов
    public static UniqueList<ImgChannel> CloneChannelsList(UniqueList<ImgChannel> channels)
    {
        var clonedChannels = new UniqueList<ImgChannel>();
        foreach (var channel in channels)
            clonedChannels.Add(channel);
        return clonedChannels;
    }

    // Клонирует список стартовых значений
    public static StartValues CloneStartValues(StartValues startValues)
    {
        var clonedStartValues = new StartValues();
        foreach (var channel in startValues.GetAddedImgChannels())
            clonedStartValues[channel] = startValues[channel];
        return clonedStartValues;
    }
}
