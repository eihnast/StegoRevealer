using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics.Helpers
{
    public class NoiseCalcMethodIntervalsInRowInfo
    {
        public ImgChannel ImgChannel { get; set; }
        public int RowId { get; set; }
        public int IntervalStartId { get; set; }
        public int IntervalEndId { get; set; }
        public double Dispersion { get; set; }
    }
}
