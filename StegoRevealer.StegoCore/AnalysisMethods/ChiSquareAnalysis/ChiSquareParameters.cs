using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;

namespace StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;

/// <summary>
/// Параметры метода стегоанализа по критерию Хи-квадрат
/// </summary>
public class ChiSquareParameters
{
    private ImageHandler _image;

    /// <summary>
    /// Изображение
    /// </summary>
    public ImageHandler Image
    {
        get => _image;
        set
        {
            if (BlockWidth == _image.Width || BlockWidth > value.Width)
                BlockWidth = value.Width;
            if (BlockHeight == _image.Height || BlockHeight > value.Height)
                BlockHeight = value.Height;
            _image = value;
            UpdateBlocks(false);
        }
    }

    /// <summary>
    /// Матрица блоков изображения
    /// </summary>
    public ImageBlocks ImgBlocks { get; private set; }

    /// <summary>
    /// Визуализировать подозрительную область
    /// </summary>
    public bool Visualize { get; set; } = false;

    /// <summary>
    /// Вертикальный обход по массиву пикселей<br/>
    /// Горизонтальный обход: сначала слева направо, потом сверху вниз<br/>
    /// Вертикальный обход: сначала сверху вниз, потом слева направо
    /// </summary>
    public TraverseType TraverseType { get; set; } = TraverseType.Horizontal;

    /// <summary>
    /// Применять ли алгоритм по отдельности для каждого канала
    /// </summary>
    public bool UseSeparateChannelsCalc { get; set; } = true;

    /// <summary>
    /// Описывает варинт формирования массива Carr - ColorsArray<br/>
    /// false: будут отдельно считаться количество интенсивности цветов в каждом канале<br/>
    /// true: будет считаться общее количество интенсивности цветов без учёта канала
    /// </summary>
    public bool UseUnitedCnum { get; set; } = true;

    /// <summary>
    /// Учитывать ли подсчёт цветов предыдущих блоков при оценке текущего (режим подсчёта с накоплением)<br/>
    /// (повышение чувствительности при низкой плотности встраивания и устойчивости к локальным флуктуациям в блоках, более надёжная оценка теоретических частот)
    /// </summary>
    public bool UsePreviousCnums { get; set; } = true;

    /// <summary>
    /// Исключать ли из анализа пары, где ожидаемая величина (частота цвета) (соответсвенно, и наблюдаемая) = 0
    /// </summary>
    public bool ExcludeZeroPairs { get; set; } = true;

    /// <summary>
    /// Объединять ли низкочастотные категории (массивов ожидаемых и наблюдаемых частот цветов) вместе
    /// </summary>
    public bool UseUnifiedCathegories { get; set; } = true;

    /// <summary>
    /// Категори с частотой ниже либо равной этой будут объединены
    /// </summary>
    public int UnifyingCathegoriesThreshold { get; set; } = 4;

    /// <summary>
    /// Порог значения p-value<br/>
    /// Если p-value выше порога, фиксируется обнаружение встраивания
    /// </summary>
    public double Threshold { get; set; } = 0.95;

    /// <summary>
    /// Анализируемые каналы
    /// </summary>
    public UniqueList<ImgChannel> Channels { get; }
        = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };


    // Параметры блоков - влияют на формирование матрицы блоков

    private int _blockWidth;
    private int _blockHeight;

    /// <summary>
    /// Ширина анализируемого блока
    /// </summary>
    public int BlockWidth
    {
        get => _blockWidth;
        set
        {
            _blockWidth = value;
            UpdateBlocks();
        }
    }

    /// <summary>
    /// Высота анализируемого блока
    /// </summary>
    public int BlockHeight
    {
        get => _blockHeight;
        set
        {
            _blockHeight = value;
            UpdateBlocks();
        }
    }


    public bool UseIncreasedCnum { get; set; } = true;


    public ChiSquareParameters(ImageHandler image)
    {
        _image = image;
        _blockWidth = Image.Width;  // По умолчанию анализ ведётся по строкам
        _blockHeight = 1;

        var blockParameters = new ImageBlocksParameters(Image, BlockWidth, BlockHeight);
        ImgBlocks = new ImageBlocks(blockParameters);
    }


    // Вспомогательные методы

    /// <summary>
    /// Пересоздаёт матрицу блоков, если ширина или высота блока изменились
    /// </summary>
    private void UpdateBlocks(bool onlyForAnotherSizes = true)
    {
        if (!onlyForAnotherSizes || (BlockWidth != ImgBlocks.BlockWidth || BlockHeight != ImgBlocks.BlockHeight))
        {
            var blockParameters = new ImageBlocksParameters(Image, BlockWidth, BlockHeight);
            ImgBlocks = new ImageBlocks(blockParameters);
        }
    }

    /// <summary>
    /// Возвращает параметры обхода для текущих параметров метода стегоанализа
    /// </summary>
    public BlocksTraverseOptions GetTraversalOptions() =>
        new BlocksTraverseOptions(channels: Channels, traverseType: TraverseType);
}
