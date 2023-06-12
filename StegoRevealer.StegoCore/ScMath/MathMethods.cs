using Accord.Statistics.Testing;
using Accord.Math;

namespace StegoRevealer.StegoCore.ScMath
{
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
        public static (double statistic, double pValue) ChiSqr(double[] expected, double[] observed,
            int? degreesOfFreedom = null)
        {
            if (expected.Length < 2)
                degreesOfFreedom = 1;
            else if (degreesOfFreedom is null || degreesOfFreedom == 0)
                degreesOfFreedom = expected.Length - 1;

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
        public static double[,] Dct(double[,] matrix)
        {
            double[,] dctMatrix = (double[,])matrix.Clone();
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    dctMatrix[i, j] -= 128;
            CosineTransform.DCT(dctMatrix);
            return dctMatrix;
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
    }
}
