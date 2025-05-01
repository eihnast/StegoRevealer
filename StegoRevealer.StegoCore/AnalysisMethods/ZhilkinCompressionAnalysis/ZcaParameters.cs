using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.ZhilkinCompressionAnalysis;

/// <summary>
/// Параметры метода стегоанализа ZCA
/// </summary>
public class ZcaParameters
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
    /// Порог средней разности степени сжатия для определения встраивания
    /// </summary>
    public double RatioThreshold { get; set; } = 0.008;

    /// <summary>
    /// Использовать ли сжатие всего изображения (а не поканально)
    /// </summary>
    public bool UseOverallCompression { get; set; } = true;
    
    /// <summary>
    /// Алгоритм сжатия
    /// </summary>
    public CompressingAlgorithm CompressingAlgorithm { get; set; } = CompressingAlgorithm.ZIP;


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


    public ZcaParameters(ImageHandler image)
    {
        _image = image;
        _blockWidth = 16;
        _blockHeight = 16;

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
    /// Автоматически вычисляет порог. При изменении размеров блока необходимо вызвать заново.
    /// </summary>
    public void SetAutomaticThreshold()
    {
        if (Image is not null)
        {
            double estimatedNoiseLevel = EstimateNoise();
            int pixelsPerBlock = BlockHeight * BlockWidth;

            double deltaThreshold = 0.004 + 0.001 * Math.Log(pixelsPerBlock) + 0.002 * estimatedNoiseLevel;
            RatioThreshold = deltaThreshold;
        }
    }

    private double EstimateNoise()
    {
        var imar = Image.ImgArray;
        int height = Image.Height;
        int width = Image.Width;

        // Horizontal
        long horizDiffSum = 0;
        long horizCount = 0;

        void calcHorizontal()
        {
            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width - 1; x++)
                {
                    foreach (var channel in Channels)
                    {
                        int channelId = (int)channel;
                        long difSum = Math.Abs(imar[y, x][channelId] - imar[y, x + 1][channelId]);
                        lock (_lock)
                        {
                            horizDiffSum += difSum;
                            horizCount++;
                        }
                    }
                }
            });
        }

        // Vertical
        long vertDiffSum = 0;
        long vertCount = 0;

        void calcVertical()
        {
            Parallel.For(0, height - 1, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    foreach (var channel in Channels)
                    {
                        int channelId = (int)channel;
                        long difSum = Math.Abs(imar[y, x][channelId] - imar[y + 1, x][channelId]);
                        lock (_lock)
                        {
                            vertDiffSum += difSum;
                            vertCount++;
                        }
                    }
                }
            });
        }

        var calucaltionTasks = new List<Task>
        {
            Task.Run(calcHorizontal),
            Task.Run(calcVertical)
        };
        Task.WaitAll(calucaltionTasks);

        double averageDiff = (double)(horizDiffSum + vertDiffSum) / (horizCount + vertCount);
        return Math.Min(1.0, averageDiff / 64.0);  // Нормализация: 64 ~= умеренный шум
    }
    private readonly object _lock = new object();
}
