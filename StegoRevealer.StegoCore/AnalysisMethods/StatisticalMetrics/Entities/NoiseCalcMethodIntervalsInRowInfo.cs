using StegoRevealer.StegoCore.CommonLib.Entities;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Entities;

public class NoiseCalcMethodIntervalsInRowInfo : ImageHorizontalIntervalInfo
{
    public double Dispersion { get; set; }
}
