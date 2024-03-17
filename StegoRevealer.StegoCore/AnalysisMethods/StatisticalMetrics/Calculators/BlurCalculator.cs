using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.ScMath;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Calculators;

public class BlurCalculator
{
    private StatmParameters _params;

    public BlurCalculator(StatmParameters parameters)
    {
        _params = parameters;
    }

    // Вычисление оценки размытости
    public double CalcBlur()
    {
        var imar = _params.Image.ImgArray;
        var gimar = PixelsTools.ToGrayscale(imar, _params.SharpnessCalcUseAveragedGrayscale);

        var guassKernelK1 = MathMethods.GenerateGuassianKernel(_params.FilterSizeK1, _params.BlurFilterGuassianKernelSigma);
        var guassKernelK2 = MathMethods.GenerateGuassianKernel(_params.FilterSizeK2, _params.BlurFilterGuassianKernelSigma);

        var blurredImarB1 = PixelsTools.DoubleToByteMatrix(MathMethods.Convolution(PixelsTools.ByteToDoubleMatrix(gimar), guassKernelK1));
        var blurredImarB2 = PixelsTools.DoubleToByteMatrix(MathMethods.Convolution(PixelsTools.ByteToDoubleMatrix(gimar), guassKernelK2));

        int MN = imar.Height * imar.Width;
        double C = 0.0;
        for (int y = 0; y < imar.Height; y++)
            for (int x = 0; x < imar.Width; x++)
                C += Math.Abs((double)(blurredImarB1[y, x] - blurredImarB2[y, x]) / MN);

        return C;
    }
}
