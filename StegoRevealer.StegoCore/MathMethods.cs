using Accord.Statistics.Testing;
using Accord.Math;
using StegoRevealer.StegoCore.ScMath;

namespace StegoRevealer.StegoCore
{
    public static class MathMethods
    {
        // Вычисление критерия Хи-квадрат
        public static (double statistic, double pValue) ChiSqr(double[] expected, double[] observed, 
            int? degreesOfFreedom = null)
        {
            if (expected.Length < 2)
                degreesOfFreedom = 1;
            else if (degreesOfFreedom is null || degreesOfFreedom == 0)
                degreesOfFreedom = expected.Length - 1;

            // var chisqr = new ChiSquareTest(expected, observed, degreesOfFreedom.Value);  // Accord version
            var chisqr = ChiSquareApache.ChiSquareTest(expected, observed);
            return (chisqr.Statistic, chisqr.PValue);
        }

        //// ???
        //public static double ChiFromFreqs(double[] expected, double[] observed)
        //{
        //    double sum = 0.0;
        //    for (int i = 0; i < observed.Length; ++i)
        //    {
        //        sum += ((observed[i] - expected[i]) *
        //          (observed[i] - expected[i])) / expected[i];
        //    }
        //    return sum;
        //}

        // Вычисление ДКП
        public static double[,] Dct(double[,] matrix)
        {
            double[,] dctMatrix = (double[,])matrix.Clone();
            CosineTransform.DCT(dctMatrix);
            return dctMatrix;
        }

        // Вычисление обратного ДКП
        public static double[,] Idct(double[,] dctMatrix)
        {
            double[,] matrix = (double[,])dctMatrix.Clone();
            CosineTransform.IDCT(matrix);
            return matrix;
        }

        // Вычисление разницы модулей
        public static int GetModulesDiff(int val1, int val2)
        {
            return Math.Abs(val1) - Math.Abs(val2);
        }

        public static int GetModulesDiff((int val1, int val2) values)
        {
            return GetModulesDiff(values.val1, values.val2);
        }

        public static double GetModulesDiff(double val1, double val2)
        {
            return Math.Abs(val1) - Math.Abs(val2);
        }

        public static double GetModulesDiff((double val1, double val2) values)
        {
            return GetModulesDiff(values.val1, values.val2);
        }

        // Вычисление модуля разницы модулей
        public static int GetModulesOfModuleDiffs(int val1, int val2)
        {
            return Math.Abs(GetModulesDiff(val1, val2));
        }

        public static int GetModulesOfModuleDiffs((int val1, int val2) values)
        {
            return GetModulesOfModuleDiffs(values.val1, values.val2);
        }

        public static double GetModulesOfModuleDiffs(double val1, double val2)
        {
            return Math.Abs(GetModulesDiff(val1, val2));
        }

        public static double GetModulesOfModuleDiffs((double val1, double val2) values)
        {
            return GetModulesOfModuleDiffs(values.val1, values.val2);
        }
    }
}
