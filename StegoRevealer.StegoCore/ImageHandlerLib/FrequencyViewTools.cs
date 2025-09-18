using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.Exceptions;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.ScMath;
using StegoRevealer.StegoCore.StegoMethods;
using static System.Net.Mime.MediaTypeNames;

namespace StegoRevealer.StegoCore.ImageHandlerLib;

/// <summary>
/// Общие инструменты работы с частотным представлением изображения
/// </summary>
public static class FrequencyViewTools
{
    // Преобразования, связанные с частотным представлением

    /// <summary>
    /// Получение ДКП-блока
    /// </summary>
    public static double[,] DctBlock(byte[,] block, int? blockSize = null)
    {
        if (blockSize is null)
            blockSize = block.GetLength(0);

        double[,] doubleBlock = new double[blockSize.Value, blockSize.Value];
        for (int i = 0; i < blockSize.Value; i++)
            for (int j = 0; j < blockSize.Value; j++)
                doubleBlock[i, j] = Convert.ToDouble(block[i, j]);

        MathMethods.Dct(doubleBlock);
        return doubleBlock;
    }

    /// <summary>
    /// Получение ДКП-блока
    /// </summary>
    public static double[,] DctBlock(ImageArray imar, BlockCoords blockCoords, int channelId, int? blockSize = null)
    {
        int yLength = blockCoords.Rd.Y - blockCoords.Lt.Y + 1;
        int xLength = blockCoords.Rd.X - blockCoords.Lt.X + 1;
        if (xLength != yLength)
            throw new IncorrectValueException($"Block must be square. But yLength = {yLength}, xLength = {xLength}");

        if (blockSize is null)
            blockSize = yLength;

        double[,] doubleBlock = new double[blockSize.Value, blockSize.Value];
        for (int y = blockCoords.Lt.Y; y <= blockCoords.Rd.Y; y++)
            for (int x = blockCoords.Lt.X; x <= blockCoords.Rd.X; x++)
                doubleBlock[y - blockCoords.Lt.Y, x - blockCoords.Lt.X] = Convert.ToDouble(imar[y, x, channelId]);

        MathMethods.Dct(doubleBlock);
        return doubleBlock;
    }

    /// <summary>
    /// Получение ОДКП-блока
    /// </summary>
    public static double[,] IDctBlock(double[,] block, int? blockSize = null)
    {
        return MathMethods.Idct(block);
    }

    /// <summary>
    /// Возвращает нормализованное значение в диапазоне [0, 255]
    public static byte NormalizeValue(double value)
    {
        if (value >= 255.0)
            return 255;
        if (value <= 0.0)
            return 0;

        return Convert.ToByte(Math.Round(value));
    }

    /// <summary>
    /// Приведение ОДКП блока к массиву дискретных значений [0, 255]
    /// </summary>
    public static byte[,] NormalizeBlock(double[,] block)
    {
        var (height, width) = (block.GetLength(0), block.GetLength(1));
        var normalizedBlock = new byte[height, width];

        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                normalizedBlock[i, j] = NormalizeValue(block[i, j]);

        return normalizedBlock;
    }

    /// <summary>
    /// Получение нормализованного ОДКП-блока
    /// </summary>
    public static byte[,] IDctBlockAndNormalize(double[,] block, int? blockSize = null)
    {
        var idctBlock = IDctBlock(block);
        return NormalizeBlock(idctBlock);
    }


    // Работа с блоком

    /// <summary>
    /// Возвращает значения коэффициентов блока из переданного блока
    /// </summary>
    public static (int val1, int val2) GetBlockCoeffs(int[,] block, ScIndexPair coeffs)
    {
        return (block[coeffs.FirstIndex, coeffs.SecondIndex], block[coeffs.SecondIndex, coeffs.FirstIndex]);
    }

    /// <summary>
    /// Возвращает значения коэффициентов блока из переданного блока
    /// </summary>
    public static (double val1, double val2) GetBlockCoeffs(double[,] block, ScIndexPair coeffs)
    {
        return (block[coeffs.FirstIndex, coeffs.SecondIndex], block[coeffs.SecondIndex, coeffs.FirstIndex]);
    }

    /// <summary>
    /// Возвращает значения коэффициентов блока по переданным координатам и массиву пикселей
    /// </summary>
    public static (int val1, int val2) GetBlockCoeffs(ScPointCoords coords, ScIndexPair coeffs,
        ImageArray imar)
    {
        ScIndexPair realCoeffs = GetCoefIndexesInImgArray(coords, coeffs);
        return (imar[realCoeffs.FirstIndex, realCoeffs.SecondIndex, coords.ChannelId],
            imar[realCoeffs.SecondIndex, realCoeffs.FirstIndex, coords.ChannelId]);
    }

    public static ScIndexPair GetCoefIndexesInImgArray(ScPointCoords coords, ScIndexPair coeffs) =>
        new ScIndexPair(coords.Y + coeffs.FirstIndex, coords.X + coeffs.SecondIndex);

    /// <summary>
    /// Возвращает модифицированные коэффициенты, скрывая в них бит (согласно порогу)<br/>
    /// Метод перенесён из StegoAnalyzer Core (kz_common.py --> get_modified_coeffs)
    /// </summary>
    [Obsolete("Устаревшая и некорректная версия встраивания по Коха-Жао (вариант Конаховича). Используйте GetModifiedCoeffsOriginal.")]
    public static (double val1, double val2) GetModifiedCoeffs(
        (double val1, double val2) coeffs, double threshold, bool incrementFirst)
    {
        var (coefVal1, coefVal2) = coeffs;  // Модифицируемые значения коэффициентов
        var difference = MathMethods.GetModulesDiff(coefVal1, coefVal2);  // Разница значений коэффициентов

        if (incrementFirst)
        {
            while (difference <= threshold)
            {
                coefVal1++;
                if (coefVal2 > 0)
                    coefVal2--;
                difference = MathMethods.GetModulesDiff(coefVal1, coefVal2);
            }
        }
        else
        {
            while (difference >= threshold)
            {
                coefVal2++;
                if (coefVal1 > 0)
                    coefVal1--;
                difference = MathMethods.GetModulesDiff(coefVal1, coefVal2);
            }
        }

        if (coeffs.val1 < 0)
            coefVal1 = -coefVal1;
        if (coeffs.val2 < 0)
            coefVal2 = -coefVal2;

        return (coefVal1, coefVal2);
    }

    /// <summary>
    /// Возвращает модифицированные коэффициенты, скрывая в них бит согласно правилам оригинальной статьи Коха и Жао:<br/>
    /// Если 1, то |v1| > |v2| + threshold<br/>
    /// Если 0, то |v2| > |v1| + threshold<br/>
    /// </summary>
    public static (double val1, double val2) GetModifiedCoeffsOriginal(
        (double val1, double val2) coeffs, double threshold, bool bit)
    {
        double v1 = coeffs.val1;
        double v2 = coeffs.val2;

        double a = Math.Abs(v1);
        double b = Math.Abs(v2);

        if (threshold < 0) threshold = 0; // на всякий случай

        if (!bit)
        {
            // Требование: |v2| - |v1| > threshold
            if (b - a > threshold) return (v1, v2);

            double need = threshold - (b - a);      // >= 0
            double d = need / 2.0;

            double aNew, bNew;
            if (a >= d)
            {
                // Проекция на прямую b' - a' = threshold в квадранте a'≥0,b'≥0
                aNew = a - d;
                bNew = b + d;
            }
            else
            {
                // Зажим у нуля по a': оптимум на луче a'=0, b'≥threshold
                aNew = 0.0;
                bNew = Math.Max(b, threshold);
            }

            // Строгое ">" — если ровно на границе, слегка увеличим |v2|
            if (!(bNew - aNew > threshold))
                bNew = ModifyingCoeffsNextAwayFromZero(v2 >= 0 ? +bNew : -bNew, increaseAbs: true) * Math.Sign(bNew == 0 ? 1 : (v2 >= 0 ? 1 : -1)) is var tmp
                       ? Math.Abs(tmp) : bNew; // (см. хелпер ниже)

            // Восстановление знаков с минимальной дельтой: уменьшаем |v1| к нулю, увеличиваем |v2| от нуля
            double v1New = aNew == 0 ? 0.0 : Math.Sign(v1) * aNew;
            double v2New = Math.Sign(v2 == 0 ? 1 : v2) * bNew;

            return (v1New, v2New);
        }
        else
        {
            // Требование: |v1| - |v2| > threshold
            if (a - b > threshold) return (v1, v2);

            double need = threshold - (a - b);      // >= 0
            double d = need / 2.0;

            double aNew, bNew;
            if (b >= d)
            {
                aNew = a + d;
                bNew = b - d;
            }
            else
            {
                bNew = 0.0;
                aNew = Math.Max(a, threshold);
            }

            if (!(aNew - bNew > threshold))
                aNew = ModifyingCoeffsNextAwayFromZero(v1 >= 0 ? +aNew : -aNew, increaseAbs: true) * Math.Sign(aNew == 0 ? 1 : (v1 >= 0 ? 1 : -1)) is var tmp
                       ? Math.Abs(tmp) : aNew;

            double v1New = Math.Sign(v1 == 0 ? 1 : v1) * aNew;
            double v2New = bNew == 0 ? 0.0 : Math.Sign(v2) * bNew;

            return (v1New, v2New);
        }
    }

    private static double ModifyingCoeffsNextAwayFromZero(double x, bool increaseAbs)
    {
        if (double.IsNaN(x) || double.IsInfinity(x)) return x;
        if (x == 0.0) return increaseAbs ? double.Epsilon : -0.0;

        return x > 0
            ? (increaseAbs ? ModifyingCoeffsNextUp(x) : ModifyingCoeffsNextDown(x))
            : (increaseAbs ? ModifyingCoeffsNextDown(x) : ModifyingCoeffsNextUp(x));
    }
    private static double ModifyingCoeffsNextUp(double x)
    {
        if (double.IsNaN(x) || x == double.PositiveInfinity) return x;
        if (x == 0.0) return double.Epsilon;
        long bits = BitConverter.DoubleToInt64Bits(x);
        if (x > 0) bits++; else bits--;
        return BitConverter.Int64BitsToDouble(bits);
    }
    private static double ModifyingCoeffsNextDown(double x)
    {
        if (double.IsNaN(x) || x == double.NegativeInfinity) return x;
        if (x == 0.0) return -double.Epsilon;
        long bits = BitConverter.DoubleToInt64Bits(x);
        if (x > 0) bits--; else bits++;
        return BitConverter.Int64BitsToDouble(bits);
    }

    /// <summary>
    /// Формирует последовательность модулей разниц модулей указанных коэффициентов в блоках матриц ДКП
    /// </summary>
    public static IEnumerable<double> GetCSequence(IEnumerable<double[,]> dctBlocks, ScIndexPair indexes) =>
        dctBlocks.Select(dctBlock => GetAbsDiff(dctBlock, indexes));

    /// <summary>
    /// Возвращает линейный список блоков матриц ДКП
    /// </summary>
    public static IEnumerable<double[,]> GetDctBlocks(
        ImageHandler img, 
        BlocksTraverseOptions? traverseOptions = null, 
        int blockSize = 8, 
        TraverseType traverseType = TraverseType.Horizontal)
    {
        if (traverseOptions is null)
            traverseOptions = new BlocksTraverseOptions(
                channels: new UniqueList<ImgChannel> { ImgChannel.Blue },
                startBlocks: new StartValues((ImgChannel.Blue, 0)),
                traverseType: traverseType,
                interlaceChannels: false,
                seed: null);

        var blocks = new ImageBlocks(new ImageBlocksParameters(img, blockSize));
        var iterator = BlocksTraverseHelper.GetForLinearAccessOneChannelBlocksIndexes(blocks, traverseOptions);

        return iterator.Select(coords => DctBlock(img.ImgArray, blocks[coords.Y, coords.X], coords.ChannelId, blockSize));
    }

    /// <summary>
    /// Считает модуль разницы модулей коэффициентов в блоке
    /// </summary>
    /// <param name="block">Блок</param>
    /// <param name="coeffs">Индексы коэффициентов блока</param>
    public static double GetAbsDiff(double[,] block, ScIndexPair coeffs)
    {
        (double value1, double value2) = (block[coeffs.FirstIndex, coeffs.SecondIndex], block[coeffs.SecondIndex, coeffs.FirstIndex]);
        return Math.Abs(MathMethods.GetModulesDiff(value1, value2));
    }
}
