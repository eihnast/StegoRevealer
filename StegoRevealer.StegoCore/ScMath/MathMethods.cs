using Accord.Statistics.Testing;
using Accord.Math;

namespace StegoRevealer.StegoCore.ScMath;

/// <summary>
/// Математические методы
/// </summary>
public static class MathMethods
{
    /// <summary>
    /// Вычисление критерия Хи-квадрат
    /// </summary>
    /// <param name="expected">Ожидаемые значения</param>
    /// <param name="observed">Наблюдаемые значения</param>
    /// <param name="degreesOfFreedom">Количество степеней свободы</param>
    public static (double statistic, double pValue) ChiSqr(List<double> expected, List<double> observed, int? degreesOfFreedom = null)
    {
        if (expected.Count < 2)
            degreesOfFreedom = 1;
        else if (degreesOfFreedom is null || degreesOfFreedom == 0)
            degreesOfFreedom = expected.Count - 1;

        // var chisqr = new ChiSquareTest(expected, observed, degreesOfFreedom.Value);  // Accord version
        var chisqr = ChiSquareApache.ChiSquareTest(expected, observed);  // Apache version
        return (chisqr.Statistic, chisqr.PValue);
    }


    /*
        Матрицы для проверки работы ДКП и ОДКП
        https://www.math.cuhk.edu.hk/~lmlui/dct.pdf
        double[,] m = new double[8, 8]
        {
            { 154, 123, 123, 123, 123, 123, 123, 136 },
            { 192, 180, 136, 154, 154, 154, 136, 110 },
            { 254, 198, 154, 154, 180, 154, 123, 123 },
            { 239, 180, 136, 180, 180, 166, 123, 123 },
            { 180, 154, 136, 167, 166, 149, 136, 136 },
            { 128, 136, 123, 136, 154, 180, 198, 154 },
            { 123, 105, 110, 149, 136, 136, 180, 166 },
            { 110, 136, 123, 123, 123, 136, 154, 136 },
        };
        double[,] m2 = (double[,])m.Clone();
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                m2[i, j] -= 128;
        CosineTransform.DCT(m);
        CosineTransform.DCT(m2);       
    
        Результат с уменьшением на 128:
            162.25 40.60 20.00 72.33 30.25 12.48 -19.65 -11.50 
            30.48 108.42 10.47 32.29 27.70 -15.50 18.41 -2.00 
            -94.14 -60.05 12.30 -43.42 -31.29 6.07 -3.33 7.14 
            -38.57 -83.36 -5.41 -22.17 -13.52 15.49 -1.33 3.53 
            -31.25 17.93 -5.52 -12.36 14.25 -5.96 11.49 -6.02 
            -0.86 -11.76 12.78 0.18 28.07 12.57 8.35 2.94 
            4.63 -2.42 12.17 6.56 -18.70 -12.75 7.70 12.03 
            -9.95 11.19 7.81 -16.29 21.46 0.02 5.91 10.68 
        Результат без уменьшения на 128:
            1186.25 40.60 20.00 72.33 30.25 12.48 -19.65 -11.50 
            30.48 108.42 10.47 32.29 27.70 -15.50 18.41 -2.00 
            -94.14 -60.05 12.30 -43.42 -31.29 6.07 -3.33 7.14 
            -38.57 -83.36 -5.41 -22.17 -13.52 15.49 -1.33 3.53 
            -31.25 17.93 -5.52 -12.36 14.25 -5.96 11.49 -6.02 
            -0.86 -11.76 12.78 0.18 28.07 12.57 8.35 2.94 
            4.63 -2.42 12.17 6.56 -18.70 -12.75 7.70 12.03 
            -9.95 11.19 7.81 -16.29 21.46 0.02 5.91 10.68 
    */

    /// <summary>
    /// Вычисление ДКП (двумерное)
    /// </summary>
    /// <param name="matrix">Матрица значений</param>
    public static void Dct(double[,] matrix)
    {
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                matrix[i, j] -= 128;
        CosineTransform.DCT(matrix);
    }

    /// <summary>
    /// Вычисление обратного ДКП (двумерное)
    /// </summary>
    /// <param name="dctMatrix">Матрица значений ДКП</param>
    public static double[,] Idct(double[,] dctMatrix)
    {
        double[,] matrix = (double[,])dctMatrix.Clone();
        CosineTransform.IDCT(matrix);
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                matrix[i, j] += 128;
        return matrix;
    }


    /// <summary>
    /// Вычисление разницы модулей
    /// </summary>
    public static int GetModulesDiff(int val1, int val2)
    {
        return Math.Abs(val1) - Math.Abs(val2);
    }

    /// <summary>
    /// Вычисление разницы модулей
    /// </summary>
    public static int GetModulesDiff((int val1, int val2) values)
    {
        return GetModulesDiff(values.val1, values.val2);
    }

    /// <summary>
    /// Вычисление разницы модулей
    /// </summary>
    public static double GetModulesDiff(double val1, double val2)
    {
        return Math.Abs(val1) - Math.Abs(val2);
    }

    /// <summary>
    /// Вычисление разницы модулей
    /// </summary>
    public static double GetModulesDiff((double val1, double val2) values)
    {
        return GetModulesDiff(values.val1, values.val2);
    }


    /// <summary>
    /// Вычисление модуля разницы модулей
    /// </summary>
    public static int GetModulesOfModuleDiffs(int val1, int val2)
    {
        return Math.Abs(GetModulesDiff(val1, val2));
    }

    /// <summary>
    /// Вычисление модуля разницы модулей
    /// </summary>
    public static int GetModulesOfModuleDiffs((int val1, int val2) values)
    {
        return GetModulesOfModuleDiffs(values.val1, values.val2);
    }

    /// <summary>
    /// Вычисление модуля разницы модулей
    /// </summary>
    public static double GetModulesOfModuleDiffs(double val1, double val2)
    {
        return Math.Abs(GetModulesDiff(val1, val2));
    }

    /// <summary>
    /// Вычисление модуля разницы модулей
    /// </summary>
    public static double GetModulesOfModuleDiffs((double val1, double val2) values)
    {
        return GetModulesOfModuleDiffs(values.val1, values.val2);
    }

    /// <summary>
    /// Возвращает среднее арифметическое последовательности
    /// </summary>
    public static double Average(double[] values)
    {
        double average = 0.0;
        for (int i = 0; i < values.Length; i++)
            average += values[i];
        average /= values.Length;
        return average;
    }

    /// <summary>
    /// Возвращает среднее арифметическое последовательности
    /// </summary>
    public static byte Average(byte[] values)
    {
        double average = 0.0;
        for (int i = 0; i < values.Length; i++)
            average += values[i];
        average /= values.Length;
        average = Math.Max(0, Math.Min(average, 255));
        return (byte)average;
    }

    /// <summary>
    /// Возвращает среднее арифметическое матрицы
    /// </summary>
    public static double Average(double[,] values)
    {
        double average = 0.0;
        for (int i = 0; i < values.GetLength(0); i++)
            for (int j = 0; j < values.GetLength(1); j++)
                average += values[i, j];
        average /= values.Length;
        return average;
    }

    /// <summary>
    /// Возвращает дисперсию последовательности
    /// </summary>
    public static double Dispersion(double[] values)
    {
        double result = 0.0;
        double average = Average(values);

        for (int i = 0; i < values.Length; i++)
            result += Math.Pow(values[i] - average, 2);
        result /= values.Length;

        return result;
    }

    /// <summary>
    /// Возвращает дисперсию последовательности
    /// </summary>
    public static double Dispersion(byte[] values)
    {
        var doubleValues = new double[values.Length];
        for (int i = 0; i < values.Length; i++)
            doubleValues[i] = (double)values[i];
        return Dispersion(doubleValues);
    }

    /// <summary>
    /// Возвращает дисперсию матрицы
    /// </summary>
    public static double Dispersion(double[,] values)
    {
        double result = 0.0;
        double average = Average(values);

        for (int i = 0; i < values.GetLength(0); i++)
            for (int j = 0; j < values.GetLength(1); j++)
                result += Math.Pow(values[i, j] - average, 2);
        result /= values.Length;

        return result;
    }

    /// <summary>
    /// Возвращает дисперсию матрицы
    /// </summary>
    public static double Dispersion(byte[,] values)
    {
        var doubleValues = new double[values.GetLength(0), values.GetLength(1)];
        for (int i = 0; i < values.GetLength(0); i++)
            for (int j = 0; j < values.GetLength(1); j++)
                doubleValues[i, j] = (double)values[i, j];
        return Dispersion(doubleValues);
    }

    /// <summary>
    /// Свёртка - Convolution
    /// </summary>
    public static double[,] Convolution(double[,] values, double[,] kernel)
    {
        int height = values.GetLength(0);
        int width = values.GetLength(1);

        var result = new double[height, width];

        int kernelOffset = kernel.GetLength(0) / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double val = 0.0;

                for (int kernelY = 0; kernelY < kernel.GetLength(0); kernelY++)
                {
                    for (int kernelX = 0; kernelX < kernel.GetLength(1); kernelX++)
                    {
                        int pixelY = y + kernelY - kernelOffset;
                        int pixelX = x + kernelX - kernelOffset;
                        if (pixelY >= height || pixelY < 0 || pixelX >= width || pixelX < 0)
                            continue;

                        val += values[y, x] * kernel[kernelY, kernelX];
                    }
                }

                result[y, x] = val;
            }
        }

        return result;
    }

    /// <summary>
    /// Формирует ядро фильтра Гаусса
    /// </summary>
    public static double[,] GenerateGuassianKernel(int size, double sigma = 1.0)
    {
        var kernel = new double[size, size];

        int k = size / 2;  // size == 2k + 1
        double n = 1 / (2 * Math.PI * Math.Pow(sigma, 2));
        double down = 2 * Math.Pow(sigma, 2);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                double up = Math.Pow((i + 1) - (k + 1), 2) + Math.Pow((j + 1) - (k + 1), 2);
                kernel[i, j] = n * Math.Exp(-(up / down));
            }
        }

        return kernel;
    }
}
