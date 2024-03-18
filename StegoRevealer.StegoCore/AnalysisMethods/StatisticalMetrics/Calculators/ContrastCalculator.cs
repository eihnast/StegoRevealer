using MathNet.Numerics.Statistics;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ScMath;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;

public class ContrastCalculator
{
    private StatmParameters _params;

    public ContrastCalculator(StatmParameters parameters)
    {
        _params = parameters;
    }

    public double CalcContrast()
    {
        var imar = _params.Image.ImgArray;
        var gimar = PixelsTools.ToGrayscale(imar, _params.ContrastCalcUseAveragedGrayscale);

        // Оценки локального контраста по формуле Гордона
        var localContrasts = new List<double>();
        for (int y = 0; y < imar.Height; y++)
        {
            for (int x = 0; x < imar.Width; x++)
            {
                var centerPixels = GetWindowCenterPixels(gimar, new Sc2DPoint(y, x));
                var surroundPixels = GetWindowSurroundPixels(gimar, new Sc2DPoint(y, x));

                double p = MathMethods.Average(centerPixels.ToArray());
                double a = MathMethods.Average(surroundPixels.ToArray());

                double C = Math.Abs(p - a) / (p + a);
                localContrasts.Add(C);
            }
        }

        // Предварительная обработка данных
        // Убедимся, что все значения данных строго положительны,
        // так как распределение Вейбулла определено только для x > 0
        var filteredData = Array.FindAll(localContrasts.ToArray(), x => x > 0);

        // Первый шаг - оценка параметров методом моментов или другими способами
        // В этом примере используем встроенные функции MathNet для работы с распределениями,
        // хотя для оценки параметров распределения Вейбулла напрямую такой функционал может быть не предоставлен
        // Используем логарифмическое преобразование данных для упрощения расчётов
        var logData = Array.ConvertAll(filteredData, Math.Log);

        // Расчёт среднего логарифмированного значения
        var meanLogData = Statistics.Mean(logData);

        // Расчёт среднего значения логарифмированных данных
        var meanData = Statistics.Mean(filteredData);

        // Расчёт логарифма среднего значения данных
        var logMeanData = Math.Log(meanData);

        // Оценка параметра формы (k) используя метод моментов
        var shapeEstimate = (logMeanData - meanLogData) * Math.Sqrt(6) / Math.PI;

        // Возвращаем оцененный параметр формы
        return shapeEstimate;
    }

    private IEnumerable<byte> GetWindowCenterPixels(byte[,] pixelsArray, Sc2DPoint point)
    {
        int height = pixelsArray.GetLength(0);
        int width = pixelsArray.GetLength(1);

        int offset = _params.ContrastCalcWindowCenterSize / 2;
        var pixels = new List<byte>();

        for (int y = Math.Max(0, point.Y - offset); y <= Math.Min(height - 1, point.Y + offset); y++)
            for (int x = Math.Max(0, point.X - offset); x <= Math.Min(width - 1, point.X + offset); x++)
                pixels.Add(pixelsArray[y, x]);

        return pixels;
    }

    private IEnumerable<byte> GetWindowSurroundPixels(byte[,] pixelsArray, Sc2DPoint point)
    {
        int height = pixelsArray.GetLength(0);
        int width = pixelsArray.GetLength(1);

        int centerOffset = _params.ContrastCalcWindowCenterSize / 2;
        int surroundOffset = _params.ContrastCalcWindowCenterSize;
        var pixels = new List<byte>();

        for (int y = Math.Max(0, point.Y - surroundOffset); y <= Math.Min(height - 1, point.Y + surroundOffset); y++)
            for (int x = Math.Max(0, point.X - surroundOffset); x <= Math.Min(width - 1, point.X + surroundOffset); x++)
                if (!(y >= point.Y - centerOffset && y <= point.Y + centerOffset && x >= point.X - centerOffset && x <= point.X + centerOffset))
                    pixels.Add(pixelsArray[y, x]);

        return pixels;
    }
}
