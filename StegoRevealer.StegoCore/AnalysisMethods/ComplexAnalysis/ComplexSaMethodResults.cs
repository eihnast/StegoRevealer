using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.StegoCore.AnalysisMethods.StatisticalMetrics;

namespace StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;

public class ComplexSaMethodResults
{
    public ChiSquareResult? ChiSquareHorizontalResult { get; set; } = null;
    public ChiSquareResult? ChiSquareVerticalResult { get; set; } = null;

    public RsResult? RsResult { get; set; } = null;

    public KzhaResult? KzhaHorizontalResult { get; set; } = null;
    public KzhaResult? KzhaVerticalResult { get; set; } = null;

    public StatmResult? StatmResult { get; set; } = null;

    public int? PixelsNum { get; set; } = null;
}
