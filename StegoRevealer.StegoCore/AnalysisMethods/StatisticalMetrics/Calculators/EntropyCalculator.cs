using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;

public class EntropyCalculator
{
    private StatmParameters _params;

    public EntropyCalculator(StatmParameters parameters)
    {
        _params = parameters;
    }

    // Метрика Цаллиса
    public double CalcTsallisEntropy()
    {
        var imar = _params.Image.ImgArray;
        var gimar = PixelsTools.ToGrayscale(imar, _params.EntropyCalcUseAveragedGrayscale);

        var histoValues = Enumerable.Repeat(0, 256).ToArray();  // Nk
        for (int y = 0; y < imar.Height; y++)
            for (int x = 0; x < imar.Width; x++)
                histoValues[gimar[y, x]]++;

        int pixelsNum = imar.Width * imar.Height;
        var pValues = new double[256];
        for (int i = 0; i < 256; i++)
            pValues[i] = (double)histoValues[i] / pixelsNum;

        double entropy = 0.0;
        double sum = 0.0;
        for (int i = 0; i < 256; i++)
            sum += Math.Pow(pValues[i], _params.EntropyCalcSensitivity);

        entropy = (1 - sum) * (1 / (_params.EntropyCalcSensitivity - 1));
        return Math.Abs(entropy);
    }

    // Энтропия Вайда - вариация энтропии Капюра с бета = 1
    public double CalcVaidaEntropy()
    {
        var imar = _params.Image.ImgArray;
        var gimar = PixelsTools.ToGrayscale(imar, _params.EntropyCalcUseAveragedGrayscale);

        var histoValues = Enumerable.Repeat(0, 256).ToArray();  // Nk
        for (int y = 0; y < imar.Height; y++)
            for (int x = 0; x < imar.Width; x++)
                histoValues[gimar[y, x]]++;

        int pixelsNum = imar.Width * imar.Height;
        var pValues = new double[256];
        for (int i = 0; i < 256; i++)
            pValues[i] = (double)histoValues[i] / pixelsNum;

        double entropy = 0.0;
        double up = 0.0, down = 0.0;
        for (int i = 0; i < 256; i++)
        {
            if (pValues[i] > 0)
            {
                up += Math.Pow(pValues[i], _params.EntropyCalcSensitivity);
                down += pValues[i];
            }
        }

        entropy = (up / down) * Math.Pow(Math.Pow(2, 1 - _params.EntropyCalcSensitivity) - 1, -1);
        return Math.Abs(entropy);
    }

    // Энтропия Капюра
    // Отброшены: Хаварда-Чарвата, Реньи, Шеннона.
}
