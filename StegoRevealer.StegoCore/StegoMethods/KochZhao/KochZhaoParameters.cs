using System.Collections;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao;

/*
 * Текущая реализация класса параметров Коха-Жао не отслеживает корреляцию между битовой длиной данных для скрытия и
 * доступным количеством блоков с учётом выбора стартовых пикселей. Таким образом, битовая длина информации, записанной
 * в параметры, может быть больше, чем доступное для скрытия число блоков. При процедуре скрытия следует это отслеживать.
 */

/// <summary>
/// Параметры метода Коха-Жао
/// </summary>
public class KochZhaoParameters : StegoMethodParams, IParams
{
    public int BlockSize { get; } = 8;  // Линейный размер блока матрицы ДКП


    /// <inheritdoc/>
    public override StegoOperationType StegoOperation { get; set; } = StegoOperationType.Hiding;

    /// <inheritdoc/>
    public override int? Seed { get; set; } = null;

    /// <inheritdoc/>
    public override UniqueList<ImgChannel> Channels { get; }
        = new UniqueList<ImgChannel> { ImgChannel.Blue };

    /// <inheritdoc/>
    public override bool InterlaceChannels { get; set; } = true;

    /// <inheritdoc/>
    public override TraverseType TraverseType { get; set; } = TraverseType.Horizontal;

    /// <summary>
    /// Порог для разницы коэффициентов скрытия
    /// </summary>
    public double Threshold { get; set; } = 120;

    /// <summary>
    /// Коэффициенты матрицы ДКП для скрытия
    /// </summary>
    public ScIndexPair HidingCoeffs { get; set; } = HidingCoefficients.Coeff45;


    // Данные для скрытия
    private string _data = string.Empty;

    /// <inheritdoc/>
    public override string Data
    {
        get
        {
            return _data;
        }
        set
        {
            if (StegoOperation is StegoOperationType.Hiding)
            {
                _data = value;
                _dataAsBitArray = StringBitsTools.StringToBitArray(value, linearBitArrays: true);
            }
        }
    }

    private BitArray _dataAsBitArray = new BitArray(0);

    /// <inheritdoc/>
    public override BitArray DataBitArray { get { return _dataAsBitArray; } }

    /// <inheritdoc/>
    public override int DataBitLength { get { return _dataAsBitArray.Length; } }


    // Количество извлекаемых бит информации и цветовых байт изображения
    private int _toExtractBitLength = 0;

    /// <inheritdoc/>
    public override int ToExtractBitLength
    {
        get
        {
            if (_toExtractBitLength <= 0)
                _toExtractBitLength = GetAvailableBlocksNum();
            return _toExtractBitLength;
        }
        set
        {
            _toExtractBitLength = value;
        }
    }

    /// <inheritdoc/>
    public override int ToExtractColorBytesNum { get { return GetNeededColorBytesNum(ToExtractBitLength); } }


    // Блоки
    private ImageBlocks _imgBlocks;

    /// <summary>
    /// Матрица блоков изображения
    /// </summary>
    public ImageBlocks ImgBlocks { get { return _imgBlocks; } }


    // Стартовые индексы

    private StartValues _startBlocks = GetDefaultStartBlocks();

    /// <summary>
    /// Стартовые блоки процессов скрытия/извлечения
    /// </summary>
    public StartValues StartBlocks
    {
        get
        {
            return new(_startBlocks);
        }
        set
        {
            if (value.Length != Channels.Count)
                throw new ArgumentException("Number of StartPixels is not equal Channels number");
            _startBlocks = value;
        }
    }

    /// <inheritdoc/>
    public override StartValues StartPoints
    {
        get { return StartBlocks; }
        set { StartBlocks = value; }
    }


    // Конструктор

    public KochZhaoParameters(ImageHandler imgHandler) : base(imgHandler)
    {
        var blockParameters = new ImageBlocksParameters(Image, BlockSize);
        _imgBlocks = new ImageBlocks(blockParameters);
    }


    // Вспомогательные методы

    /// <inheritdoc/>
    public override void Reset()
    {
        Seed = null;
        InterlaceChannels = true;

        Threshold = 120;

        Channels.Clear();
        Channels.Add(ImgChannel.Blue);

        _toExtractBitLength = 0;
        Data = "";

        var blockParameters = new ImageBlocksParameters(Image, BlockSize);
        _imgBlocks = new ImageBlocks(blockParameters);
        _startBlocks = GetDefaultStartBlocks();
    }

    // Метод неактуален для метода Коха-Жао, но требуется интерфейсом и может быть использован для составления метрик
    /// <summary>
    /// Количество цветовых байт, которое необходимо для сокрытия/извлечения всей информации (с учётом размера блоков)<br/>
    /// Цветовой байт - значения 8 бит пикселя в одном канале
    /// </summary>
    public override int GetNeededColorBytesNum(int? dataBitLength = null)
    {
        if (dataBitLength is null)
            dataBitLength = DataBitLength;
        return Convert.ToInt32(Math.Round((double)dataBitLength * _imgBlocks.BlocksNum, MidpointRounding.ToPositiveInfinity));
    }


    /// <summary>
    /// Количество блоков, которое необходимо для сокрытия (извлечения) всей информации (с учётом размера блоков)
    /// </summary>
    public int GetNeededBlocksNum(int? dataBitLength = null)
    {
        if (dataBitLength is null)
            dataBitLength = DataBitLength;
        var allBlocksNum = GetAllBlocksNum();
        return dataBitLength.Value >= allBlocksNum ? allBlocksNum : dataBitLength.Value;
    }

    /// <summary>
    /// Количество доступных для скрытия блоков с учётом стартовых индексов блоков
    /// </summary>
    public int GetAvailableBlocksNum()
    {
        int allBlocksInChannelNum = _imgBlocks.BlocksInRow * _imgBlocks.BlocksInRow;

        int blocksNum = 0;
        foreach (var channel in Channels)
        {
            int startBlock = StartBlocks[channel];
            blocksNum += (allBlocksInChannelNum - startBlock);
        }

        return blocksNum;
    }


    /// <summary>
    /// Количество всех блоков (с учётом всех каналов)
    /// </summary>
    public int GetAllBlocksNum() => _imgBlocks.BlocksNum * Channels.Count;

    /// <summary>
    /// Возвращает стандарный список стартовых пикселей
    /// </summary>
    private static StartValues GetDefaultStartBlocks() => new StartValues((ImgChannel.Blue, 0));
}
