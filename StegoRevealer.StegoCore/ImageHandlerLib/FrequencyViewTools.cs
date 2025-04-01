using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.Exceptions;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib.Blocks;
using StegoRevealer.StegoCore.ScMath;

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
}
