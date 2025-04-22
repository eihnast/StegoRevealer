using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;

namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod;

/// <summary>
/// Параметры метода стегоанализа по критерию Хи-квадрат
/// </summary>
public class RsParameters
{
    /// <summary>
    /// Изображение
    /// </summary>
    public ImageHandler Image { get; set; }

    /// <summary>
    /// Анализируемые каналы
    /// </summary>
    public UniqueList<ImgChannel> Channels { get; }
        = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

    /// <summary>
    /// Матрица блоков изображения
    /// </summary>
    public ImageBlocks ImgBlocks { get; private set; }

    /// <summary>
    /// Вертикальный обход по массиву пикселей<br/>
    /// Горизонтальный обход: сначала слева направо, потом сверху вниз<br/>
    /// Вертикальный обход: сначала сверху вниз, потом слева направо
    /// </summary>
    public TraverseType TraverseType { get; set; } = TraverseType.Horizontal;

    /// <summary>
    /// Длина групп пикселей
    /// </summary>
    [Obsolete("Необходимо использовать стандартный механизм определения блоков: BlockWidth и BlockHeight")]
    public int PixelsGroupLength { get; set; } = RsHelper.DefaultPixelsGroupLength;

    /// <summary>
    /// Маска флиппинга
    /// </summary>
    public int[] FlippingMask { get; set; } = RsHelper.DefaultFlippingMask; 

    /// <summary>
    /// Функция регулярности
    /// </summary>
    public Func<IEnumerable<int>, int> RegularityFunction { get; set; } = RsHelper.DefaultRegularityFunction;

    /// <summary>
    /// Функция прямого флиппинга
    /// </summary>
    public Func<int, int> FlipDirect { get; set; } = RsHelper.DefaultFlipDirect;

    /// <summary>
    /// Функция обратного флиппинга
    /// </summary>
    public Func<int, int> FlipBack { get; set; } = RsHelper.DefaultFlipBack;

    /// <summary>
    /// Функция нулевого флиппинга
    /// </summary>
    public Func<int, int> FlipNone { get; set; } = RsHelper.DefaultFlipNone;


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


    public RsParameters(ImageHandler image)
    {
        Image = image;
        _blockWidth = 4;  // По умолчанию анализ ведётся по 4 пикселя подряд в строке
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

    /// <summary>
    /// Устанавливает стандартный формат блока: 4 подряд идущих по горизонтали пикселя 
    /// </summary>
    public void SetDefaultLinearBlock()
    {
        BlockWidth = 4;
        BlockHeight = 1;
    }

    /// <summary>
    /// Устанавливает стандартный формат блока: блоки 2x2 пикселя
    /// </summary>
    public void SetDefaultSquareBlock()
    {
        BlockWidth = 2;
        BlockHeight = 2;
    }
}
