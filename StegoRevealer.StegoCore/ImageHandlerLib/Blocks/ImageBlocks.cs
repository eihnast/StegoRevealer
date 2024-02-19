using StegoRevealer.StegoCore.CommonLib.ScTypes;

namespace StegoRevealer.StegoCore.ImageHandlerLib.Blocks;

public class ImageBlocks
{
    private ImageBlocksParameters _parameters;  // Параметры для разбиения на блоки
    private ImageHandler _img;  // Изображение

    // Матрица блоков: хранятся индексы левого верхнего и правого нижнего пикселя каждого блока
    private BlockCoords[,] _blocksMatrix;


    /// <summary>
    /// Количество блоков в строке - ширина матрицы блоков
    /// </summary>
    public int BlocksInRow { get; private set; } = 0;


    /// <summary>
    /// Количество блоков в столбце - высота матрицы блоков
    /// </summary>
    public int BlocksInColumn { get; private set; } = 0;

    public int BlockWidth { get => _parameters.BlockWidth; }
    public int BlockHeight { get => _parameters.BlockHeight; }
    public ImageHandler Image { get => _img; }


    /// <summary>
    /// Общее количество блоков
    /// </summary>
    public int BlocksNum { get { return BlocksInColumn * BlocksInRow; } }


    #pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
    public ImageBlocks(ImageBlocksParameters parameters)
    {
        _parameters = parameters;
        _img = _parameters.Image;
        UpdateMatrix();
    }
    #pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.


    /// <summary>
    /// Обновление матрицы блоков с изменёнными параметрами
    /// </summary>
    public void UpdateMatrix(bool updateImage = false)
    {
        if (updateImage)
            _img = _parameters.Image;

        // Размеры матрицы блоков
        BlocksInRow = _img.Width / _parameters.BlockWidth;
        BlocksInColumn = _img.Height / _parameters.BlockHeight;

        // Учёт возможных неполных блоков, если не установлен флаг учёта только целых блоков
        if (!_parameters.OnlyWholeBlocks)
        {
            if (_img.Width % _parameters.BlockWidth != 0)
                BlocksInRow++;
            if (_img.Height % _parameters.BlockHeight != 0)
                BlocksInColumn++;
        }

        // Формирование матрицы блоков
        _blocksMatrix = new BlockCoords[BlocksInColumn, BlocksInRow];
        for (int y = 0; y < BlocksInColumn; y++)
        {
            for (int x = 0; x < BlocksInRow; x++)
            {
                // Левая верхняя координата всегда существует при обходе
                // Но если блок последний, его реальная ширина и длина могут быть меньше BlockHeight и BlockWidth -
                // в том случае, если выключен OnlyWholeBlocks

                var lt = new Sc2DPoint(y * _parameters.BlockHeight, x * _parameters.BlockWidth);
                var rd = new Sc2DPoint(
                    Math.Min(lt.Y + _parameters.BlockHeight - 1, _img.Height - 1),
                    Math.Min(lt.X + _parameters.BlockWidth - 1, _img.Width - 1)
                    );
                _blocksMatrix[y, x] = new BlockCoords(lt, rd);
            }
        }
    }


    // Индексаторы

    /// <summary>
    /// Доступ по индексатору матрицы блоков
    /// </summary>
    public BlockCoords this[int y, int x]
    {
        get { return _blocksMatrix[y, x]; }
    }


    // Вспомогательные статичные методы
    public static List<ScPixel> MapBlockToPixelsList(byte[,,] block, UniqueList<ImgChannel> channels)
    {
        var list = new List<ScPixel>();
        for (int i = 0; i < block.GetLength(0); i++)
        {
            for (int j = 0; j < block.GetLength(1); j++)
            {
                var pixel = new ScPixel();
                foreach (var channel in channels)
                    pixel[(int)channel] = block[i, j, (int)channel];
                list.Add(pixel);
            }
        }

        return list;
    }
}
