using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;
using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;

public class EntropyCalculator
{
    private readonly StatmParameters _params;

    private readonly double[] _pValues;

    public EntropyData CalcEntropy()
    {
        var methods = _params.EntropyMethods;
        return new EntropyData
        {
            Shennon = methods.HasFlag(EntropyMethods.All) || methods.HasFlag(EntropyMethods.Shennon) ? CalcShennonEntropy() : 0.0,
            Vaida = methods.HasFlag(EntropyMethods.All) || methods.HasFlag(EntropyMethods.Vaida) ? CalcVaidaEntropy() : 0.0,
            Tsallis = methods.HasFlag(EntropyMethods.All) || methods.HasFlag(EntropyMethods.Tsallis) ? CalcTsallisEntropy() : 0.0,
            Renyi = methods.HasFlag(EntropyMethods.All) || methods.HasFlag(EntropyMethods.Renyi) ? CalcRenyiEntropy() : 0.0,
            Havard = methods.HasFlag(EntropyMethods.All) || methods.HasFlag(EntropyMethods.Havard) ? CalcHavardEntropy() : 0.0
        };
    }

    public EntropyCalculator(StatmParameters parameters)
    {
        _params = parameters;

        var imar = _params.Image.ImgArray;
        var gimar = PixelsTools.ToGrayscale(imar, _params.EntropyCalcUseAveragedGrayscale);

        var histoValues = Enumerable.Repeat(0, 256).ToArray();  // Nk
        for (int y = 0; y < imar.Height; y++)
            for (int x = 0; x < imar.Width; x++)
                histoValues[gimar[y, x]]++;

        int pixelsNum = imar.Width * imar.Height;
        _pValues = new double[256];
        for (int i = 0; i < 256; i++)
            _pValues[i] = (double)histoValues[i] / pixelsNum;
    }

    // Метрика Цаллиса
    public double CalcTsallisEntropy()
    {
        double entropy = 0.0;
        double sum = 0.0;
        for (int i = 0; i < 256; i++)
            sum += Math.Pow(_pValues[i], _params.EntropyCalcSensitivity);

        entropy = (1 - sum) * (1 / (_params.EntropyCalcSensitivity - 1));
        return entropy;
    }

    // Энтропия Вайда - вариация энтропии Капюра с бета = 1
    public double CalcVaidaEntropy()
    {
        double entropy = 0.0;
        double up = 0.0, down = 0.0;
        for (int i = 0; i < _pValues.Length; i++)
        {
            if (_pValues[i] > 0)
            {
                up += Math.Pow(_pValues[i], _params.EntropyCalcSensitivity);
                down += _pValues[i];
            }
        }

        entropy = (up / down) * Math.Pow(Math.Pow(2, 1 - _params.EntropyCalcSensitivity) - 1, -1);
        return 1 / Math.Abs(entropy);
    }

    // Энтропия Шеннона
    public double CalcShennonEntropy()
    {
        double entropy = 0.0;
        for (int i = 0; i < _pValues.Length; i++)
        {
            if (_pValues[i] > 0)
            {
                entropy -= _pValues[i] * Math.Log2(_pValues[i]);
            }
        }

        return entropy;
    }

    // Энтропия Реньи
    public double CalcRenyiEntropy(double? alpha = null)
    {
        alpha ??= _params.EntropyCalcSensitivity;

        double entropy = 0.0;
        double sum = 0.0;
        for (int i = 0; i < _pValues.Length; i++)
        {
            if (_pValues[i] > 0)
                sum += Math.Pow(_pValues[i], alpha.Value);
        }

        entropy = Math.Log2(sum) * (1 / (1 - alpha.Value));
        return entropy;
    }

    // Энтропия Хаварда-Чарвата
    public double CalcHavardEntropy()
    {
        double entropy = 0.0;
        double sum = 0.0;
        for (int i = 0; i < _pValues.Length; i++)
        {
            if (_pValues[i] > 0)
                sum += Math.Pow(_pValues[i], _params.EntropyCalcSensitivity);
        }

        entropy = (sum - 1) * (1 / (Math.Pow(2, 1 - _params.EntropyCalcSensitivity) - 1));
        return entropy;
    }
}
